using System;
using Match3.Controller;
using Match3.Data;
using Match3.Model.Generation;
using Match3.Model.Gravity;
using Match3.Model.Matching;
using Match3.Model.Settings;
using Match3.Model.Special;
using Match3.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Match3.Installers
{
    public sealed class Match3SceneInstaller : IInstaller
    {
        private const string BoardSettingsPath = "Match3/BoardSettings";
        private const string ScoreSettingsPath = "Match3/ScoreSettings";

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(Load<BoardSettings>(BoardSettingsPath)).As<IBoardSettings>();
            builder.RegisterInstance(Load<ScoreSettings>(ScoreSettingsPath)).As<IScoreSettings>();
            builder.Register<MatchFinder>(Lifetime.Singleton).As<IMatchFinder>();
            builder.Register<MoveScanner>(Lifetime.Singleton).As<IMoveScanner>();
            builder.RegisterInstance(new System.Random());
            builder.Register<BoardGenerator>(Lifetime.Singleton).As<IBoardGenerator>();
            builder.Register<GravityResolver>(Lifetime.Singleton).As<IGravityResolver>();
            builder.Register<SpecialTileEffects>(Lifetime.Singleton);
            builder.Register<ChainResolver>(Lifetime.Singleton);
            builder.Register<SpecialCombinationResolver>(Lifetime.Singleton);
            builder.Register<BoardGeometry>(Lifetime.Singleton);
            builder.Register<TilePool>(Lifetime.Singleton);
            builder.Register<BoardView>(Lifetime.Singleton);
            builder.Register<BoardFeedbackView>(Lifetime.Singleton);
            builder.Register<BoardController>(Lifetime.Singleton);
            builder.Register<InputController>(Lifetime.Singleton);
            builder.RegisterComponentOnNewGameObject<BoardInputView>(Lifetime.Singleton);
            builder.RegisterBuildCallback(container => container.Resolve<BoardView>());
            builder.RegisterBuildCallback(container => container.Resolve<BoardFeedbackView>());
            builder.RegisterBuildCallback(container => container.Resolve<BoardController>());
            builder.RegisterBuildCallback(container => container.Resolve<InputController>());
            builder.RegisterBuildCallback(container => container.Resolve<BoardInputView>());
        }

        private static TSettings Load<TSettings>(string path) where TSettings : ScriptableObject
        {
            TSettings settings = Resources.Load<TSettings>(path);
            if (settings == null)
            {
                throw new NullReferenceException($"No {typeof(TSettings).Name} asset at Resources/{path}.");
            }

            return settings;
        }
    }
}
