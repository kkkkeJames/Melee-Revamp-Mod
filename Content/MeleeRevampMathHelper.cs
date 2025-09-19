using Microsoft.Xna.Framework;
using System;

namespace MeleeRevamp.Content
{
    public static class MeleeRevampMathHelper
    {
        //Helper function calculating magnitude of vector
        public static float VectorMagnitudeHelper(Vector2 vec)
        {
            return (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
        }
        //Helper function calculating the distance between 2 vectors
        public static float EucilidDistanceHelper(Vector2 a, Vector2 b)
        {
            return (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }
        //Pythagorean helper function
        public static float PythagoreanHelper(float a, float b)
        {
            return (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }
        //Helper function calculating the radius of an ellipse with its long/short radius and a rotation
        public static float EllipseRadiusHelper(float a, float b, float rot)
        {
            return (float)Math.Sqrt(Math.Pow(a, 2) * Math.Pow(b, 2) / (Math.Pow(a, 2) * Math.Pow(Math.Sin(rot), 2) + Math.Pow(b, 2) * Math.Pow(Math.Cos(rot), 2)));
        }
        //Helper function calculating the value of a logistic model
        public static float LogisticHelper(float a, float b, float pow, float red, float x)
        {
            return (float)(a / (1 + (a / b - 1) * Math.Pow(Math.E, pow * x)) - red);
        }
    }
}
