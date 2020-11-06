//TODO Make better settings
//TODO Add sound
//TODO Medals

//TODO Make better settings
//TODO Medals

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Program;

namespace Flappy_Bird
{
    class Game : MainRenderWindow
    {
        private struct Bird
        {
            public Vector2 Pos;
            public Vector2 Vel;

            public void Update(Vector2 force, float fElapsedTime)
            {
                Vel += force * fElapsedTime;
                Pos += Vel;
            }
        }

        private class Pipe
        {
            public float Gap;
            public float XPos;
            public float YOff;
            public bool HasScored;
        }


        private Bird _bird;
        private List<Pipe> _pipe = new List<Pipe> ();
        private int _score, _lastY;
        private float _tOff, _bOff, _curDif = 0.35f, _cur, _angle, _x, _diePlace;
        private bool _tag = true, _isPaused, _hasStarted, _hasDied, _hasHitFloor, _hasRestarted = true;
        private Texture _pipeTexture, _background, _floor;
        private Font _arial, _flappy;



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
            _bird = new Bird { Pos = new Vector2(Width / 2, Height / 2), Vel = new Vector2(0f, 0f) };
            _arial = new Font("assets/arial.fnt", "assets/arial_0.png");
            _flappy = new Font("assets/flappy.fnt", "assets/flappy_0.png");
            _pipeTexture = new Texture("assets/pipe-green.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            _floor = new Texture("assets/base.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            _bOff = Width;
            _hasHitFloor = false;

            set.addButton("Exit Game", 10, 10, 180, 40, Color4.Blue, () => { Exit(); return 1; }, _arial);
            set.addButton("Change Background", 10, 60, 180, 40, Color4.Blue, () =>
            {
                if (Convert.ToString(set.settings["Background Texture"]) == "assets/background-day.png")
                {
                    set.settings["Background Texture"] = "assets/background-night.png";
                }
                else
                {
                    set.settings["Background Texture"] = "assets/background-day.png";

                }
                _background = new Texture(Convert.ToString(set.settings["Background Texture"]), TextureMinFilter.Nearest, TextureMagFilter.Nearest);
                return 1;
            }, _arial);

            set.readSettings();
            _background = new Texture(Convert.ToString(set.settings["Background Texture"]), TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            base.OnLoad(e);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"Flappy Bird, FPS:{1f / e.Time}";
            Clear();
            RenderBackground();
            foreach (Pipe p in _pipe)
            {
                ShowPipe(p, _bird);
            }
            RenderBase();
            ShowBird(_bird);
            
            //Score Text
            var l = getPhraseLength(Convert.ToString(_score), 72, _flappy);
            l = (Width - l) / 2;
            drawText(Convert.ToString(_score), 72, l, (Height / 2) + 300, _flappy, Color4.White);
            
            l = getPhraseLength(Convert.ToString(set.settings["High Score"]), 36, _flappy);
            l = (Width - l) / 2;
            drawText(Convert.ToString(set.settings["High Score"]), 36, l, (Height / 2) + 228, _flappy, Color4.Yellow);
            base.OnRenderFrame(e);
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _isPaused = showSet;
            MouseState mouse;
            KeyboardState keyboard;
            if (!_isPaused && _hasStarted && !_hasDied)
            {
                float change = 500f * (float)e.Time;
                foreach (Pipe p in _pipe)
                {
                    p.XPos -= change;
                }
                _bOff -= change;

                for (var i = _pipe.Count - 1; i >= 0; i--)
                {
                    if (_pipe[i].XPos < -100)
                    {
                        _pipe.Remove(_pipe[i]);
                    }
                }

                if (_x >= 400)
                {
                    var yOff = RandomNumber.Between(-200, 312);
                    if(yOff - _lastY > 325)
                    {
                        yOff = _lastY + 325;
                    }
                    _pipe.Add(new Pipe { Gap = 200f + RandomNumber.Between(-25, 25), XPos = Width, YOff = yOff });
                    _lastY = yOff;
                    _x = 0;
                }
                _x += change;


                if (Focused)
                {
                    mouse = Mouse.GetState();
                    keyboard = Keyboard.GetState();
                    if ((mouse.IsButtonDown(MouseButton.Left) || keyboard.IsKeyDown(Key.Space)) && _tag)
                    {
                        _bird.Vel.Y = +7.5f;
                        _cur = 30;
                        _tag = false;
                        Task.Run(() => { PlaySound("assets/wing.wav"); });
                    }
                    if (!mouse.IsButtonDown(MouseButton.Left) && !keyboard.IsKeyDown(Key.Space))
                    {
                        _tag = true;
                    }

                }
                _bird.Update(new Vector2(0f, -20f), (float)e.Time);

            } 
            else if(_hasDied && !_hasHitFloor)
            {
                _bird.Update(new Vector2(0f, -20f), (float)e.Time);
            }
            else if (!_hasDied)
            {
                if (Focused)
                {
                    mouse = Mouse.GetState();
                    keyboard = Keyboard.GetState();
                    if ((mouse.IsButtonDown(MouseButton.Left) || keyboard.IsKeyDown(Key.Space))  && _hasRestarted)
                    {
                        _hasStarted = true;
                        _curDif = 1;
                    }
                }

                if(_cur <= 0)
                {
                    _cur = 30;
                }

                if (!showSet)
                {
                    float change = 500f * (float)e.Time;
                    _bOff -= change;
                    _bird.Pos.Y += (float)Math.Sin(_angle);
                    _bird.Vel.Y = 0.5f * (float)Math.Sin(_angle);
                    _angle += 0.1f;
                }
            }
            else
            {
                mouse = Mouse.GetState();
                keyboard = Keyboard.GetState();
                if (mouse.IsButtonDown(MouseButton.Left) || keyboard.IsKeyDown(Key.Space))
                { 
                    RestartGame();
                }
            }
            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();
            if (!mouse.IsAnyButtonDown && !keyboard.IsAnyKeyDown)
            {
                _hasRestarted = true;
            }
            


            
            if (_score > Convert.ToInt32(set.settings["High Score"]))
            {
                set.settings["High Score"] = _score;
            }
            
            base.OnUpdateFrame(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteTexture(_pipeTexture.Handle);
            if (_score > Convert.ToInt32(set.settings["High Score"]))
            {
                set.settings["High Score"] = _score;
            }
            set.writeSettings();
            base.OnUnload(e);
        }
        private void ShowBird(Bird bird)
        {
            //drawEllipse(bird.pos.X, bird.pos.Y, 16f, 16f, Color4.Yellow);

            Vector2 lookDir = new Vector2(5, bird.Vel.Y);
            lookDir.Normalize();
            const int sizey = 16;
            const int sizex = 20;

            var a = Math.Acos(Vector2.Dot(lookDir, new Vector2(0, -1)));
            a -= Math.PI / 2;
            Vector2 p1 = new Vector2((float)(Math.Cos(a) * (-sizex)) - (float)(Math.Sin(a) * (-sizey)), (float)(Math.Sin(a) * (-sizex)) + (float)(Math.Cos(a) * (-sizey)));
            Vector2 p2 = new Vector2((float)(Math.Cos(a) * (sizex)) - (float)(Math.Sin(a) * (-sizey)), (float)(Math.Sin(a) * (sizex)) + (float)(Math.Cos(a) * (-sizey)));
            Vector2 p3 = new Vector2((float)(Math.Cos(a) * (-sizex)) - (float)(Math.Sin(a) * (sizey)), (float)(Math.Sin(a) * (-sizex)) + (float)(Math.Cos(a) * (sizey)));
            Vector2 p4 = new Vector2((float)(Math.Cos(a) * (sizex)) - (float)(Math.Sin(a) * (sizey)), (float)(Math.Sin(a) * (sizex)) + (float)(Math.Cos(a) * (sizey)));

            string path;

            if(_cur >= 20)
            {
                path = "assets/yellowbird-downflap.png";
            }
            else if(_cur >= 10)
            {
                path = "assets/yellowbird-midflap.png";
            }
            else 
            {
                path = "assets/yellowbird-upflap.png";
            } 


            drawTexturedQuad(bird.Pos.X + p4.X, bird.Pos.Y + p4.Y, 1, 0, 1, 
                             bird.Pos.X + p2.X, bird.Pos.Y + p2.Y, 1, 0, 0,
                             bird.Pos.X + p1.X, bird.Pos.Y + p1.Y, 1, 1, 0,
                             bird.Pos.X + p3.X, bird.Pos.Y + p3.Y, 1, 1, 1,
                             path, Color4.White, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            if (_cur != 0) { _cur -= _curDif; }
        }

        private void ShowPipe(Pipe pipe, Bird bird)
        {
            Color4 col = Color4.White;
            if (CheckOverlap(16, bird.Pos.X, bird.Pos.Y, pipe.XPos, 112, pipe.XPos + 100, ((Height - 112) / 2) - (pipe.Gap / 2) + pipe.YOff))
            {
                _hasDied = true;
                
            } 

            drawTexturedRectangle(pipe.XPos, ((Height - 112) / 2) - (pipe.Gap / 2) - 800 + pipe.YOff, 0, 0, pipe.XPos + 100, ((Height - 112) / 2) - (pipe.Gap / 2) + pipe.YOff, 1, 1, _pipeTexture, col);

            col = Color4.White;
            if (CheckOverlap(16, bird.Pos.X, bird.Pos.Y, pipe.XPos, ((Height - 112) / 2) + (pipe.Gap / 2) + pipe.YOff, pipe.XPos + 100, (Height)))
            {
                _hasDied = true;
            } 
            drawTexturedRectangle(pipe.XPos, ((Height - 112) / 2) + (pipe.Gap / 2) + 800 + pipe.YOff, 0, 0, pipe.XPos + 100, ((Height - 112) / 2) + (pipe.Gap / 2) + pipe.YOff, 1 , 1,  _pipeTexture, col);

            if (pipe.XPos >= (Width / 2) - 10 && pipe.XPos <= (Width / 2) + 10 && !pipe.HasScored && !_hasDied)
            {
                _score++;
                pipe.HasScored = true;
                Task.Run(() => { PlaySound("assets/point.wav"); });
            }
    
        }

        static bool CheckOverlap(int r, float xc, float yc,
                                float x1, float y1,
                                float x2, float y2)
        {


            int xn = Math.Max((int)x1,
                     Math.Min((int)xc, (int)x2));
            int yn = Math.Max((int)y1,
                     Math.Min((int)yc, (int)y2));

            int dx = xn - (int)xc;
            int dy = yn - (int)yc;
            return (dx * dx + dy * dy) <= r * r;
        }

        private void RenderBackground()
        {
            float width = (1000f * 276f) / 512f;
            float nrect = Width / width;
            float xSize = (float)Math.Ceiling(Width / width) * width;

            drawTexturedRectangle(0, 0, _tOff, 0, xSize, Height, nrect + _tOff, 1, _background, Color4.White);
            _tOff += 0.00001f;
        }

        private void RenderBase()
        {
            float width = (1000f * 336f) / 512f;
            float nrect = Width / width;
            float xSize = (float)Math.Ceiling(Width / width) * width;
            Color4 col = Color4.White;
            if (CheckOverlap(16, _bird.Pos.X, _bird.Pos.Y, 0, 0, Width, 112))
            {
                _hasHitFloor = true;
                _hasDied = true;
            }

            drawTexturedRectangle(0, 0, nrect + ((_bOff * nrect) / xSize), 0, xSize, 112, (_bOff * nrect) / xSize, 1, _floor, col);
        }

        private void RestartGame()
        {
            _hasDied = false;
            _hasHitFloor = false;
            _hasStarted = false;
            _bird = new Bird { Pos = new Vector2(Width / 2, Height / 2), Vel = new Vector2(0f, 0f) };
            _bOff = Width;
            _pipe = new List<Pipe>();
            _tag = true;
            _hasRestarted = false;
            
            if (_score > Convert.ToInt32(set.settings["High Score"]))
            {
                set.settings["High Score"] = _score;
            }
            set.writeSettings();
            
            _score = 0;
        }

        private void PlaySound(String path)
        {
            using var audioFile = new AudioFileReader(path);
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            outputDevice.Play();
            if (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(1000);
            }
        }

    }
    
}
