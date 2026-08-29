using Match3.Data;
using Match3.Model.Settings;
using Match3.View;
using Syntac.DI.Core.Installers;
using TMPro;
using UnityEngine;
using VContainer;

namespace Match3.Installers
{
    public sealed class ScoreViewInstaller : MonoInstaller
    {
        [SerializeField] private ScoreSettings scoreSettings;
        [SerializeField] private TMP_FontAsset font;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(scoreSettings).As<IScoreSettings>();
            builder.Register<ScoreHudView>(Lifetime.Singleton).WithParameter(font);
            builder.RegisterBuildCallback(container => container.Resolve<ScoreHudView>());
        }
    }
}
