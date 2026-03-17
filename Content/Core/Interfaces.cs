using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeleeRevamp.Content.Core
{
    public interface IDrawBloom
    {         
        void DrawBloom();
    }
    public interface IDrawWarp
    {
        void DrawWarp();
    }
    public interface IDrawDissolve
    {
        void DrawDissolve();
    }
    public interface IOrderedLoadable
    {
        void Load();
        void Unload();
        float Priority { get; }
    }
}