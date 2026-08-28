using System;
using Match3.Data;
using Match3.Model.Settings;
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
