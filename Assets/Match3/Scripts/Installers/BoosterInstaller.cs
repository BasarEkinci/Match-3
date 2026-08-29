using Match3.Controller;
using Match3.Data;
using Match3.Model.Boosters;
using Match3.Model.Settings;
using Match3.View;
using Syntac.DI.Core.Installers;
using TMPro;
using UnityEngine;
using VContainer;

namespace Match3.Installers
{
    public sealed class BoosterInstaller : MonoInstaller
    {
        [SerializeField] private BoosterSettings boosterSettings;
        [SerializeField] private TMP_FontAsset font;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(boosterSettings).As<IBoosterSettings>();
            builder.Register<BoosterModel>(Lifetime.Singleton);
            builder.Register<BoosterController>(Lifetime.Singleton);
            builder.Register<BoosterHudView>(Lifetime.Singleton).WithParameter(font);
            builder.RegisterBuildCallback(container => container.Resolve<BoosterController>());
            builder.RegisterBuildCallback(container => container.Resolve<BoosterHudView>());
        }
    }
}
