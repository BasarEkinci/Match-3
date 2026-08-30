using Match3.Data;
using Match3.Model.Settings;
using Match3.View;
using Match3.Core.DI.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Match3.Installers
{
    public sealed class BoardViewInstaller : MonoInstaller
    {
        private const string SpritesParameter = "sprites";
        private const string SpecialSpritesParameter = "specialSprites";

        [SerializeField] private BoardSettings boardSettings;
        [SerializeField] private HintSettings hintSettings;
        [SerializeField] private Sprite[] gemSprites;
        [SerializeField] private Sprite[] specialSprites;
        [SerializeField] private Sprite backgroundSprite;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(boardSettings).As<IBoardSettings>();
            builder.RegisterInstance(hintSettings).As<IHintSettings>();
            builder.Register<BoardGeometry>(Lifetime.Singleton);
            builder.Register<TilePool>(Lifetime.Singleton)
                .WithParameter(SpritesParameter, gemSprites)
                .WithParameter(SpecialSpritesParameter, specialSprites);
            builder.Register<BoardBackgroundView>(Lifetime.Singleton)
                .WithParameter(backgroundSprite);
            builder.Register<BoardView>(Lifetime.Singleton);
            builder.RegisterComponentOnNewGameObject<BoardInputView>(Lifetime.Singleton);
            builder.RegisterBuildCallback(container => container.Resolve<BoardBackgroundView>());
            builder.RegisterBuildCallback(container => container.Resolve<BoardView>());
            builder.RegisterBuildCallback(container => container.Resolve<BoardInputView>());
        }
    }
}
