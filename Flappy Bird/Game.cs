using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Program;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace Flappy_Bird
{
    class Game : MainRenderWindow
    {
        struct Bird
        {
            public Vector2 pos;
            public Vector2 vel;

            public void Update(Vector2 force, float fElapsedTime)
            {
                vel += force * fElapsedTime;
                pos += vel;
            }
        }

        class Pipe
        {
            public float Gap;
            public float xPos;
            public float yOff;
            public System.Boolean hasScored = false;

            public void setXpos(float _xPos)
            {
                xPos = _xPos;
            }
        }
        Random rd = new Random();
        private Bird bird;
        private List<Pipe> pipe = new List<Pipe> ();
        int x = 0, cur = 0, score = 0;
        float tOff = 0, bOff;
        private System.Boolean tag = true;
        private Texture pipeTexture, background, floor;

        public Game(int width, int height, string title)
            : base(width, height, title)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            setClearColor(new Color4(0.0f, 0.0f, 0.0f, 1.0f)); //Sets Background Color
            UseDepthTest = false; //Enables Depth Testing for 3D
            RenderLight = false; //Makes the 3D light visible
            UseAlpha = true; //Enables alpha use
            KeyboardAndMouseInput = false; //Enables keyboard and mouse input for 3D movement
            CursorVisible = true;
            bird = new Bird { pos = new Vector2(Width / 2, Height / 2), vel = new Vector2(0f, 0f) };
            pipe.Add(new Pipe { Gap = 200f, xPos = Width, yOff = 50f});
            loadFont("assets/arial.fnt", "assets/arial_0.png");
            pipeTexture = new Texture("assets/pipe-green.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            background = new Texture("assets/background-day.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            floor = new Texture("assets/base.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            bOff = Width;
            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"Flappy Bird, FPS:{1f / e.Time}";
            Clear();
            renderBackground();
            foreach (Pipe p in pipe)
            {
                showPipe(p, bird);
            }
            renderBase();
            showBird(bird);

            drawText(String.Format("Score: {0}", score), new Vector2(0f, Height - 32), Color4.Red);
            base.OnRenderFrame(e);

        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float change = 250f * (float)e.Time;
            foreach (Pipe p in pipe)
            {
                p.xPos -= change;
            }
            bOff -= change;

            for (var i = pipe.Count - 1; i >= 0; i--)
            {
                if(pipe[i].xPos < -100)
                {
                    pipe.Remove(pipe[i]);
                }
            }

            if(x == 100)
            {
                pipe.Add(new Pipe { Gap = 200f + RandomNumber.Between(-25, 25), xPos = Width, yOff = RandomNumber.Between(-200, 312) });
                x = 0;
            }

            if (Focused)
            {
                var keyboard = Keyboard.GetState();
                if(keyboard.IsKeyDown(Key.Space) && tag)
                {
                    bird.vel.Y = +5f;
                    cur = 30;
                    tag = false;
                }
                if (!keyboard.IsAnyKeyDown)
                {
                    tag = true;
                }

            }
            bird.Update(new Vector2(0f, -10f), (float)e.Time);
            base.OnUpdateFrame(e);
            x++;
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteTexture(pipeTexture.Handle);
            base.OnUnload(e);
        }
        private void showBird(Bird bird)
        {
            //drawEllipse(bird.pos.X, bird.pos.Y, 16f, 16f, Color4.Yellow);

            Vector2 lookDir = new Vector2(5, bird.vel.Y);
            lookDir.Normalize();
            const int sizey = 16;
            const int sizex = 20;

            var a = Math.Acos(Vector2.Dot(lookDir, new Vector2(0, -1)));
            a -= Math.PI / 2;
            Vector2 p1 = new Vector2((float)(Math.Cos(a) * (-sizex)) - (float)(Math.Sin(a) * (-sizey)), (float)(Math.Sin(a) * (-sizex)) + (float)(Math.Cos(a) * (-sizey)));
            Vector2 p2 = new Vector2((float)(Math.Cos(a) * (sizex)) - (float)(Math.Sin(a) * (-sizey)), (float)(Math.Sin(a) * (sizex)) + (float)(Math.Cos(a) * (-sizey)));
            Vector2 p3 = new Vector2((float)(Math.Cos(a) * (-sizex)) - (float)(Math.Sin(a) * (sizey)), (float)(Math.Sin(a) * (-sizex)) + (float)(Math.Cos(a) * (sizey)));
            Vector2 p4 = new Vector2((float)(Math.Cos(a) * (sizex)) - (float)(Math.Sin(a) * (sizey)), (float)(Math.Sin(a) * (sizex)) + (float)(Math.Cos(a) * (sizey)));

            String path = "";

            if(cur >= 20)
            {
                path = "assets/yellowbird-downflap.png";
            }
            else if(cur >= 10)
            {
                path = "assets/yellowbird-midflap.png";
            }
            else 
            {
                path = "assets/yellowbird-upflap.png";
            } 


            drawTexturedQuad(bird.pos.X + p4.X, bird.pos.Y + p4.Y, 1, 0, 1, 
                             bird.pos.X + p2.X, bird.pos.Y + p2.Y, 1, 0, 0,
                             bird.pos.X + p1.X, bird.pos.Y + p1.Y, 1, 1, 0,
                             bird.pos.X + p3.X, bird.pos.Y + p3.Y, 1, 1, 1,
                             path, Color4.White, OpenTK.Graphics.OpenGL.TextureMinFilter.Nearest, OpenTK.Graphics.OpenGL.TextureMagFilter.Nearest);
            if (cur != 0) { cur--; }

            drawText(String.Format("Bird Heigth: {0}", bird.pos.Y), new Vector2(0f, Height - 64f), Color4.Red);
            drawText(String.Format("Bird Angle: {0}", a), new Vector2(0f, Height - 96f), Color4.Red);

        }

        private void showPipe(Pipe pipe, Bird bird)
        {
            Color4 col = Color4.White;
            if (checkOverlap(16, bird.pos.X, bird.pos.Y, pipe.xPos, 112, pipe.xPos + 100, ((Height - 112) / 2) - (pipe.Gap / 2) + pipe.yOff))
            {
                col = Color4.Red;
            }
            
            drawTexturedRectangle(pipe.xPos, ((Height - 112) / 2) - (pipe.Gap / 2) - 800 + pipe.yOff, 0, 0, pipe.xPos + 100, ((Height - 112) / 2) - (pipe.Gap / 2) + pipe.yOff, 1, 1, pipeTexture, col);

            col = Color4.White;
            if (checkOverlap(16, bird.pos.X, bird.pos.Y, pipe.xPos, ((Height - 112) / 2) + (pipe.Gap / 2) + pipe.yOff, pipe.xPos + 100, (Height)))
            {
                col = Color4.Red;
            }
            drawTexturedRectangle(pipe.xPos, ((Height - 112) / 2) + (pipe.Gap / 2) + 800 + pipe.yOff, 0, 0, pipe.xPos + 100, ((Height - 112) / 2) + (pipe.Gap / 2) + pipe.yOff, 1 , 1,  pipeTexture, col);

            if (pipe.xPos >= (Width / 2) - 2 && pipe.xPos <= (Width / 2) + 3 && !pipe.hasScored)
            {
                score++;
                pipe.hasScored = true;
            }

        }

        static bool checkOverlap(int R, float Xc, float Yc,
                                float X1, float Y1,
                                float X2, float Y2)
        {


            int Xn = Math.Max((int)X1,
                     Math.Min((int)Xc, (int)X2));
            int Yn = Math.Max((int)Y1,
                     Math.Min((int)Yc, (int)Y2));

            int Dx = (int)Xn - (int)Xc;
            int Dy = (int)Yn - (int)Yc;
            return (Dx * Dx + Dy * Dy) <= R * R;
        }

        private void renderBackground()
        {
            float width = (1000f * 276f) / 512f;
            float nrect = Width / width;
            float xSize = (float)Math.Ceiling(Width / width) * width;

            drawTexturedRectangle(0, 0, tOff, 0, xSize, Height, nrect + tOff, 1, background, Color4.White);
            tOff += 0.00001f;
        }

        private void renderBase()
        {
            float width = (1000f * 336f) / 512f;
            float nrect = Width / width;
            float xSize = (float)Math.Ceiling(Width / width) * width;
            drawText(String.Format("bOff: {0}", bOff), new Vector2(0f, Height - 128f), Color4.Red);
            drawText(String.Format("texture offset: {0}", ((bOff * nrect) / xSize)), new Vector2(0f, Height - 150f), Color4.Red);
            
            Color4 col = Color4.White;
            if (checkOverlap(16, bird.pos.X, bird.pos.Y, 0, 0, Width, 112))
            {
                col = Color4.Red;
            }

            drawTexturedRectangle(0, 0, nrect + ((bOff * nrect) / xSize), 0, xSize, 112, (bOff * nrect) / xSize, 1, floor, col);
        }
    }
}
