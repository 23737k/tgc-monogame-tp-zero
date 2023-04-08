using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Models;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Clase principal del juego.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        private GraphicsDeviceManager Graphics { get; }
        private CityScene City { get; set; }
        private Model CarModel { get; set; }

        //Agrego las matrices del auto: World, Rotation
        private Effect CarEffect { get; set; }
        private Matrix CarWorld { get; set; }
        private FollowCamera FollowCamera { get; set; }


        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Se encarga de la configuracion y administracion del Graphics Device.
            Graphics = new GraphicsDeviceManager(this);

            // Carpeta donde estan los recursos que vamos a usar.
            Content.RootDirectory = "Content";

            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        /// <summary>
        ///     Llamada una vez en la inicializacion de la aplicacion.
        ///     Escribir aca todo el codigo de inicializacion: Todo lo que debe estar precalculado para la aplicacion.
        /// </summary>
        protected override void Initialize()
        {
            // Enciendo Back-Face culling.
            // Configuro Blend State a Opaco.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Configuro las dimensiones de la pantalla.
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Graphics.ApplyChanges();

            // Creo una camara para seguir a nuestro auto.
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);

            // Configuro la matriz de mundo del auto.
            CarWorld = Matrix.Identity ;

            //inicializo me headVector
            headVector= new Vector3(0f,0f,1f);

            base.Initialize();
        }

        /// <summary>
        ///     Llamada una sola vez durante la inicializacion de la aplicacion, luego de Initialize, y una vez que fue configurado GraphicsDevice.
        ///     Debe ser usada para cargar los recursos y otros elementos del contenido.
        /// </summary>
        protected override void LoadContent()
        {
            // Creo la escena de la ciudad.
            City = new CityScene(Content);

            // La carga de contenido debe ser realizada aca.

            //Cargo el modelo del auto
            CarModel = Content.Load<Model>(ContentFolder3D + "scene/car");
            CarEffect = Content.Load<Effect>(ContentFolderEffects + "BasicShader"); //Esto siempre se pone, sino tira segmentation fault. No se porque. Preguntar en el foro

            base.LoadContent();
        }

        /// <summary>
        ///     Es llamada N veces por segundo. Generalmente 60 veces pero puede ser configurado.
        ///     La logica general debe ser escrita aca, junto al procesamiento de mouse/teclas.
        /// </summary>


        private Vector3 headVector;     //vector que apunta a la parte delantera del auto
        protected override void Update(GameTime gameTime)
        {
            Matrix Rotation= Matrix.Identity;       //La matriz identidad hace que al multiplicarla por otra no afecte el resultado -> no hay rotacion
            float speed=0;
            // Capturo el estado del teclado.
            var keyboardState = Keyboard.GetState();
            var time= Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            
            
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                // Salgo del juego.
                Exit();
            }

            if(keyboardState.IsKeyDown(Keys.Space))
            {

            }

            if(keyboardState.IsKeyDown(Keys.W))
                speed= -300*time;

            if(keyboardState.IsKeyDown(Keys.S))
                speed= 300*time;

            if(keyboardState.IsKeyDown(Keys.A))
            {
                Vector3 position= CarWorld.Translation;             //Guardo la posicion actual del auto
                Rotation = Matrix.CreateTranslation(-position) *    //Me desplazo al origen
                         Matrix.CreateRotationY(time) *             //roto
                         Matrix.CreateTranslation(position);        //regreso a la posicion original
                
                Matrix rotationMatrix = Matrix.CreateRotationY(time);                   //Creo una matriz de rotacion con la rotacion anterior
                Vector3 rotatedVector = Vector3.Transform(headVector, rotationMatrix);  //A partir del headVector y la matriz rotada obtengo un nuevo vector rotado
                headVector= Vector3.Normalize(rotatedVector);                           //Le asigno a headVector el nuevo vector rotado y lo normalizo (para que tenga modulo unitario)
                
            }                 
            if(keyboardState.IsKeyDown(Keys.D))
            {
                Vector3 position= CarWorld.Translation;
                Rotation = Matrix.CreateTranslation(-position) *
                         Matrix.CreateRotationY(-time) *
                         Matrix.CreateTranslation(position);
                
                Matrix rotationMatrix = Matrix.CreateRotationY(-time);
                Vector3 rotatedVector = Vector3.Transform(headVector, rotationMatrix);
                headVector= Vector3.Normalize(rotatedVector);
            }
           
            
            CarWorld *= Matrix.CreateTranslation(headVector*speed)*Rotation;    //Me desplazo y roto. 

            //Matrix.CreateFromQuaternion(quaternion) ;

            FollowCamera.Update(gameTime, CarWorld);

            base.Update(gameTime);
        }


        /// <summary>
        ///     Llamada para cada frame.
        ///     La logica de dibujo debe ir aca.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Limpio la pantalla.
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Dibujo la ciudad.
            City.Draw(gameTime, FollowCamera.View, FollowCamera.Projection);

            //Por ahora se dibuja asi. La matriz vista es del auto, las de vista y proyeccion son las de la camara.
            CarModel.Draw(CarWorld,FollowCamera.View, FollowCamera.Projection);

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Libero los recursos cargados.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos cargados dessde Content Manager.
            Content.Unload();

            base.UnloadContent();
        }
    }
   
}
