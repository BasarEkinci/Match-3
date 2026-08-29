using Match3.Controller;
using Match3.Data;
using Match3.Model.Generation;
using Match3.Model.Gravity;
using Match3.Model.Matching;
using Match3.Model.Persistence;
using Match3.Model.Scoring;
using Match3.Model.Special;
using VContainer;
using VContainer.Unity;

namespace Match3.Installers
{
    public sealed class Match3ModelInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(new System.Random());
            builder.Register<MatchFinder>(Lifetime.Singleton).As<IMatchFinder>();
            builder.Register<MoveScanner>(Lifetime.Singleton).As<IMoveScanner>();
            builder.Register<BoardGenerator>(Lifetime.Singleton).As<IBoardGenerator>();
            builder.Register<GravityResolver>(Lifetime.Singleton).As<IGravityResolver>();
            builder.Register<SpecialTileEffects>(Lifetime.Singleton);
            builder.Register<ChainResolver>(Lifetime.Singleton);
            builder.Register<SpecialCombinationResolver>(Lifetime.Singleton);
            builder.Register<PlayerPrefsSaveRepository>(Lifetime.Singleton).As<ISaveRepository>();
            builder.Register<ScoreModel>(Lifetime.Singleton);
            builder.Register<ScoreController>(Lifetime.Singleton);
            builder.Register<BoardController>(Lifetime.Singleton);
            builder.Register<InputController>(Lifetime.Singleton);
            builder.Register<SaveController>(Lifetime.Singleton);
            builder.RegisterBuildCallback(container => container.Resolve<ScoreController>());
            builder.RegisterBuildCallback(container => container.Resolve<BoardController>());
            builder.RegisterBuildCallback(container => container.Resolve<InputController>());
            builder.RegisterBuildCallback(container => container.Resolve<SaveController>());
        }
    }
}
