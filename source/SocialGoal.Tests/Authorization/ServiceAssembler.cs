using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SocialGoal.Data.Infrastructure;
using SocialGoal.Data.Models;
using SocialGoal.Model.Models;

namespace SocialGoal.Tests.Authorization
{
    /// <summary>
    /// Reflection-based DI-lite for the behavioral authorization matrix. Wires a
    /// controller to REAL services and repositories over ONE shared
    /// <see cref="DatabaseFactory"/> -- i.e. one <c>SocialGoalEntities</c> per
    /// invocation, so reads and writes inside a single controller call are
    /// consistent (harness design, "Wiring facts").
    ///
    /// It replaces the ~150 lines of hand-wiring the legacy controller fixtures
    /// carry and is robust to constructor changes: every service ctor takes only
    /// repositories + IUnitOfWork, every repository ctor takes IDatabaseFactory,
    /// so the whole graph resolves from the single factory. UserManager is a
    /// special registration for AccountController's ctor.
    ///
    /// This is TEST infrastructure that INVOKES the legacy graph to characterize
    /// it; it introduces none of the flagged legacy patterns itself.
    /// </summary>
    internal sealed class ServiceAssembler : IDisposable
    {
        private readonly DatabaseFactory _factory = new DatabaseFactory();
        private readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Type> _implByInterface = new Dictionary<Type, Type>();
        private readonly List<IDisposable> _extraDisposables = new List<IDisposable>();

        internal ServiceAssembler()
        {
            var assemblies = new[]
            {
                typeof(SocialGoal.Data.Repository.GoalRepository).Assembly, // SocialGoal.Data
                typeof(SocialGoal.Service.GoalService).Assembly             // SocialGoal.Service
            };

            foreach (var asm in assemblies)
            {
                foreach (var type in asm.GetTypes())
                {
                    // Some concrete repositories/services are declared `internal`
                    // (e.g. SupportRepository); reflection instantiates them via
                    // their public ctor regardless, so do not filter on visibility.
                    if (!type.IsClass || type.IsAbstract)
                        continue;
                    foreach (var iface in type.GetInterfaces())
                    {
                        if (iface.Namespace == "SocialGoal.Data.Repository"
                            || iface.Namespace == "SocialGoal.Service")
                        {
                            _implByInterface[iface] = type; // exactly one impl per interface
                        }
                    }
                }
            }
        }

        /// <summary>The single shared DbContext the whole wired graph reads/writes through.</summary>
        internal SocialGoalEntities SharedContext
        {
            get { return _factory.Get(); }
        }

        internal TController BuildController<TController>() where TController : class
        {
            return (TController)Resolve(typeof(TController));
        }

        private object Resolve(Type type)
        {
            object cached;
            if (_cache.TryGetValue(type, out cached))
                return cached;

            if (type == typeof(IDatabaseFactory))
                return Store(type, _factory);

            if (type == typeof(IUnitOfWork))
                return Store(type, new UnitOfWork(_factory));

            if (type == typeof(UserManager<ApplicationUser>))
                return Store(type, BuildUserManager());

            if (type.IsInterface)
            {
                Type impl;
                if (!_implByInterface.TryGetValue(type, out impl))
                    throw new InvalidOperationException("No implementation registered for " + type.FullName);
                var instance = Resolve(impl);
                _cache[type] = instance; // alias interface -> concrete instance
                return instance;
            }

            // Concrete class: construct via its greediest public constructor.
            var ctor = type.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .First();
            var args = ctor.GetParameters().Select(p => Resolve(p.ParameterType)).ToArray();
            var built = ctor.Invoke(args);
            return Store(type, built);
        }

        private object Store(Type type, object instance)
        {
            _cache[type] = instance;
            return instance;
        }

        // AccountController's ctor requires a UserManager. The BOLA targets under
        // test (EditProfile, Follow/Accept/Reject/Unfollow) never call it, so a
        // real manager over its own context simply satisfies the ctor. It gets a
        // dedicated SocialGoalEntities (disposed here) so controller disposal is
        // never needed to avoid leaking the shared context.
        private UserManager<ApplicationUser> BuildUserManager()
        {
            var context = new SocialGoalEntities();
            _extraDisposables.Add(context);
            var store = new UserStore<ApplicationUser>(context);
            _extraDisposables.Add(store);
            var manager = new UserManager<ApplicationUser>(store);
            _extraDisposables.Add(manager);
            return manager;
        }

        public void Dispose()
        {
            foreach (var d in _extraDisposables)
            {
                try { d.Dispose(); } catch { /* best-effort cleanup */ }
            }
            _extraDisposables.Clear();
            _factory.Dispose();
        }
    }
}
