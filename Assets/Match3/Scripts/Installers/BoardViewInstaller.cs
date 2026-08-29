using Match3.Data;
using Match3.Model.Settings;
using Match3.View;
using Syntac.DI.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Match3.Installers
{
    public sealed class BoardViewInstaller : MonoInstaller
    {
        [SerializeField] private BoardSettings boardSettings;
        [SerializeField] private Sprite[] gemSprites;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(boardSettings).As<IBoardSettings>();
            builder.Register<BoardGeometry>(Lifetime.Singleton);
            builder.Register<TilePool>(Lifetime.Singleton).WithParameter(gemSprites);
            builder.Register<BoardView>(Lifetime.Singleton);
            builder.RegisterComponentOnNewGameObject<BoardInputView>(Lifetime.Singleton);
            builder.RegisterBuildCallback(container => container.Resolve<BoardView>());
            builder.RegisterBuildCallback(container => container.Resolve<BoardInputView>());
        }
    }
}
