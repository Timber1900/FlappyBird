using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Program;

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
            public bool hasScored = false, hasHitBottom = false, hasHitTop = false;

            public void setXpos(float _xPos)
            {
                xPos = _xPos;
            }
        }
        Random rd = new Random();
        private Bird bird;
        private List<Pipe> pipe = new List<Pipe> ();
        int score = 0, miss = 0, lastY = 0;
        float tOff = 0, bOff, curDif = 0.35f, cur = 0, angle = 0f, x = 0, diePlace = 0;
        private bool tag = true, missTagBase = true, isPaused = false, hasStarted = false, hasDied = false;
        private Texture pipeTexture, background, floor;
        Font arial;
        Font flappy;

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
            useSettings = true;
            CursorVisible = true;
            bird = new Bird { pos = new Vector2(Width / 2, Height / 2), vel = new Vector2(0f, 0f) };
            pipe.Add(new Pipe { Gap = 200f, xPos = Width, yOff = 50f});
            arial = new Font("assets/arial.fnt", "assets/arial_0.png");
            flappy = new Font("assets/flappy.fnt", "assets/flappy_0.png");
            pipeTexture = new Texture("assets/pipe-green.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            background = new Texture("assets/background-day.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            floor = new Texture("assets/base.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            bOff = Width;

            set.addButton("Exit Game", 10, 10, 180, 40, Color4.Blue, () => { Exit(); return 1; }, arial);
            set.readSettings();
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

            drawText(string.Format("High Score: {0}", set.settings["High Score"]), 32, 0f, Height - 32, arial, Color4.Red);
            drawText(string.Format("Score: {0}", score), 32, 0f, Height - 64, arial, Color4.Red);
            drawText(string.Format("Miss: {0}", miss), 32, 0f, Height - 96, arial, Color4.Red);

            var l = getPhraseLength(Convert.ToString(score), 64, flappy);
            l = (Width - l) / 2;
            drawText(Convert.ToString(score), 64, l, (Height / 2) + 200, flappy, Color4.White);

            base.OnRenderFrame(e);

        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            isPaused = showSet;
            if (!isPaused && hasStarted && !hasDied)
            {
                float change = 500f * (float)e.Time;
                foreach (Pipe p in pipe)
                {
                    p.xPos -= change;
                }
                bOff -= change;

                for (var i = pipe.Count - 1; i >= 0; i--)
                {
                    if (pipe[i].xPos < -100)
                    {
                        pipe.Remove(pipe[i]);
                    }
                }

                if (x >= 400)
                {
                    var yOff = RandomNumber.Between(-200, 312);
                    if(yOff - lastY > 325)
                    {
                        yOff = lastY + 325;
                    }
                    pipe.Add(new Pipe { Gap = 200f + RandomNumber.Between(-25, 25), xPos = Width, yOff = yOff });
                    lastY = yOff;
                    x = 0;
                }
                x += change;


                if (Focused)
                {
                    var mouse = Mouse.GetState();
                    if (mouse.IsButtonDown(MouseButton.Left) && tag)
                    {
                        bird.vel.Y = +7.5f;
                        cur = 30;
                        tag = false;
                    }
                    if (!mouse.IsButtonDown(MouseButton.Left))
                    {
                        tag = true;
                    }

                }
                bird.Update(new Vector2(0f, -20f), (float)e.Time);

            } 
            else if(hasDied)
            {
                if(diePlace < 25)
                {
                    bird.pos.Y += 1;
                    bird.vel.Y += 5;
                } else if (diePlace == 25)
                {
                    bird.vel.Y = 0;

                }
                else
                {
                    bird.pos.Y -= 25;
                    bird.vel.Y -= 5;

                }
                diePlace++;
            }
            else
            {
                if (Focused)
                {
                    var mouse = Mouse.GetState();
                    if (mouse.IsButtonDown(MouseButton.Left))
                    {
                        hasStarted = true;
                        curDif = 1;
                    }
                }

                if(cur <= 0)
                {
                    cur = 30;
                }

                if (!showSet)
                {
                    float change = 500f * (float)e.Time;
                    bOff -= change;
                    bird.pos.Y += (float)Math.Sin(angle);
                    bird.vel.Y = 0.5f * (float)Math.Sin(angle);
                    angle += 0.1f;
                }

                
            }

            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Key.L) && !hasDied)
            {
                hasDied = true;
            }
            
            base.OnUpdateFrame(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteTexture(pipeTexture.Handle);
            if (score > Convert.ToInt32(set.settings["High Score"]))
            {
                set.settings["High Score"] = score;
            }
            set.writeSettings();
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

            string path = "";

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
                             path, Color4.White, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            if (cur != 0) { cur -= curDif; }

            drawText(string.Format("Bird Heigth: {0}", bird.pos.Y), 32, 0f, Height - 128f, arial, Color4.Red);
            drawText(string.Format("Bird Angle: {0}", a), 32, 0f, Height - 160f, arial, Color4.Red);

        }

        private void showPipe(Pipe pipe, Bird bird)
        {
            Color4 col = Color4.White;
            if (checkOverlap(16, bird.pos.X, bird.pos.Y, pipe.xPos, 112, pipe.xPos + 100, ((Height - 112) / 2) - (pipe.Gap / 2) + pipe.yOff))
            {
                col = Color4.Red;
                if (!pipe.hasHitTop)
                {
                    miss++;
                    pipe.hasHitTop = true;
                }
            } 

            drawTexturedRectangle(pipe.xPos, ((Height - 112) / 2) - (pipe.Gap / 2) - 800 + pipe.yOff, 0, 0, pipe.xPos + 100, ((Height - 112) / 2) - (pipe.Gap / 2) + pipe.yOff, 1, 1, pipeTexture, col);

            col = Color4.White;
            if (checkOverlap(16, bird.pos.X, bird.pos.Y, pipe.xPos, ((Height - 112) / 2) + (pipe.Gap / 2) + pipe.yOff, pipe.xPos + 100, (Height)))
            {
                col = Color4.Red;
                if (!pipe.hasHitBottom)
                {
                    miss++;
                    pipe.hasHitBottom = true;
                }
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
            drawText(String.Format("bOff: {0}", bOff), 32, 0f, Height - 192f, arial, Color4.Red);
            drawText(String.Format("texture offset: {0}", ((bOff * nrect) / xSize)), 32, 0f, Height - 224f, arial, Color4.Red);
            
            Color4 col = Color4.White;
            if (checkOverlap(16, bird.pos.X, bird.pos.Y, 0, 0, Width, 112))
            {
                col = Color4.Red;
                if (missTagBase)
                {
                    miss++;
                    missTagBase = false;
                }
            } else
            {
                missTagBase = true;
            }

            drawTexturedRectangle(0, 0, nrect + ((bOff * nrect) / xSize), 0, xSize, 112, (bOff * nrect) / xSize, 1, floor, col);
        }
    }
}
