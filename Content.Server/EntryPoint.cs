﻿using Content.Server.Dream;
using Content.Server.Input;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

namespace Content.Server {
    public class EntryPoint : GameServer {
        private IDreamManager _dreamManager;
        private DreamCommandSystem _commandSystem;

        public override void Init() {
            IComponentFactory componentFactory = IoCManager.Resolve<IComponentFactory>();
            componentFactory.DoAutoRegistrations();

            ServerContentIoC.Register();

            // This needs to happen after all IoC registrations, but before IoC.BuildGraph();
            foreach (var callback in TestingCallbacks)
            {
                var cast = (ServerModuleTestingCallbacks) callback;
                cast.ServerBeforeIoC?.Invoke();
            }

            IoCManager.BuildGraph();
            componentFactory.GenerateNetIds();
        }

        public override void PostInit() {
            _dreamManager = IoCManager.Resolve<IDreamManager>();
            _commandSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<DreamCommandSystem>();
            _dreamManager.Initialize();
        }

        protected override void Dispose(bool disposing) {
            _dreamManager.Shutdown();
        }

        public override void Update(ModUpdateLevel level, FrameEventArgs frameEventArgs) {
            _commandSystem.RunRepeatingCommands();
        }
    }
}