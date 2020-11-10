using OpenTK;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;

namespace Flappy_Bird
{
    static class Program
    {
        static void Main(string[] args)
        {
            using Game game = new Game(1000, 1000, "", 60.0);
            //Run takes a double, which is how many frames per second it should strive to reach.
            //You can leave that out and it'll just update as fast as the hardware will allow it.
            //game.WindowState = WindowState.Fullscreen;
            game.Run();
            
        }
    }
}
