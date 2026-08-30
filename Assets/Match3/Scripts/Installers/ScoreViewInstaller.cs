using Match3.Data;
using Match3.Model.Settings;
using Match3.Core.DI.Installers;
using UnityEngine;
using VContainer;

namespace Match3.Installers
{
    public sealed class ScoreViewInstaller : MonoInstaller
    {
        [SerializeField] private ScoreSettings scoreSettings;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(scoreSettings).As<IScoreSettings>();
        }
    }
}
