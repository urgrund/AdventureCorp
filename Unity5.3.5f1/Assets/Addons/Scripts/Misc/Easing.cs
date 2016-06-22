using UnityEngine;
using System;


public static class Easing
{
    public static class Linear
	{
		public static float easeIn( float t )
		{
			return t;
		}
		
		
		public static float easeOut( float t )
		{
			return t;
		}
		
		
		public static float easeInOut( float t )
		{
			return t;
		}
	}


	public static class Quartic
	{
		public static float easeIn( float t )
		{
			return Mathf.Pow( t, 4.0f );
		}
		
		
		public static float easeOut( float t )
		{
			return ( Mathf.Pow( t - 1, 4 ) - 1 ) * -1;
		}
		
		
		public static float easeInOut( float t )
		{
			if( t <= 0.5f )
				return Quartic.easeIn( t * 2 ) / 2;
			else
				return ( Quartic.easeOut( ( t - 0.5f ) * 2.0f ) / 2 ) + 0.5f;
		}
	}


	public static class Quintic
	{
		public static float easeIn( float t )
		{
			return Mathf.Pow( t, 5.0f );
		}
		
		
		public static float easeOut( float t )
		{
			return ( Mathf.Pow( t - 1, 5 ) + 1 );
		}
		
		
		public static float easeInOut( float t )
		{
			if( t <= 0.5f )
				return Quintic.easeIn( t * 2 ) / 2;
			else
				return ( Quintic.easeOut( ( t - 0.5f ) * 2.0f ) / 2 ) + 0.5f;
		}
	}


	public static class Sinusoidal
	{
		public static float easeIn( float t )
		{
			return Mathf.Sin( ( t - 1 ) * ( Mathf.PI / 2 ) ) + 1;
		}
		
		
		public static float easeOut( float t )
		{
			return Mathf.Sin( t * ( Mathf.PI / 2 ) );
		}
		
		
		public static float easeInOut( float t )
		{
			if( t <= 0.5f )
				return Sinusoidal.easeIn( t * 2 ) / 2;
			else
				return ( Sinusoidal.easeOut( ( t - 0.5f ) * 2.0f ) / 2 ) + 0.5f;
		}
	}


	public static class Exponential
	{
		public static float easeIn( float t )
		{
			return Mathf.Pow( 2, 10 * ( t - 1 ) );
		}
		
		
		public static float easeOut( float t )
		{
			return 1 - Mathf.Pow( 2, -10 * t );
		}
		
		
		public static float easeInOut( float t )
		{
			if( t <= 0.5f )
				return Exponential.easeIn( t * 2 ) / 2;
			else
				return Exponential.easeOut( t * 2 - 1 ) / 2 + 0.5f;
		}
	}


	public static class Circular
	{
		public static float easeIn( float t )
		{
			return ( -1 * Mathf.Sqrt( 1 - t * t ) + 1 );
		}
		
		
		public static float easeOut( float t )
		{
			return Mathf.Sqrt( 1 - Mathf.Pow( t - 1, 2 ) );
		}
		
		
		public static float easeInOut( float t )
		{
			if( t <= 0.5f )
				return Circular.easeIn( t * 2 ) / 2;
			else
				return ( Circular.easeOut( ( t - 0.5f ) * 2.0f ) / 2 ) + 0.5f;
		}
	}


	public static class Back
	{
		private const float s = 1.70158f;
		private const float s2 = 1.70158f * 1.525f;


		public static float easeIn( float t )
		{
			return t * t * ( ( s + 1 ) * t - 2 );
		}
		
		
		public static float easeOut( float t )
		{
			t = t - 1;
			return ( t * t * ( ( s + 1 ) * t + s ) + 1 );
		}
		
		
		public static float easeInOut( float t )
		{
			t = t * 2;
			
			if( t < 1 )
			{
				return 0.5f * ( t * t * ( ( s2 + 1 ) * t - s2 ) );
			}
			else
			{
				t -= 2;
				return 0.5f * ( t * t * ( ( s2 + 1 ) * t + s2 ) + 2 );
			}
		}
	}



    public static class Bounce
    {
        //static float d = 2f;

        public static float easeIn(float k)
        {
            return 1f - easeOut(1f - k);
        }

        public static float easeOut(float k)
        {
            if (k < (1f / 2.75f))
            {
                return 7.5625f * k * k;
            }
            else if (k < (2f / 2.75f))
            {
                return 7.5625f * (k -= (1.5f / 2.75f)) * k + 0.75f;
            }
            else if (k < (2.5f / 2.75f))
            {
                return 7.5625f * (k -= (2.25f / 2.75f)) * k + 0.9375f;
            }
            else
            {
                return 7.5625f * (k -= (2.625f / 2.75f)) * k + 0.984375f;
            }
        }

        public static float easeInOut(float k)
        {
            if (k < 0.5f) return easeIn(k * 2f) * 0.5f;
            return easeOut(k * 2f - 1f) * 0.5f + 0.5f;
        }

        //public static float easeIn(float t)
        //{
        //    return 1 - easeOut(1 - t);
        //}


        //public static float easeOut(float t)
        //{
        //    if (t < (1 / 2.75f))
        //    {
        //        return 1;
        //    }
        //    else if (t < (2 / 2.75f))
        //    {
        //        t -= (1.5f / 2.75f);
        //        return 7.5625f * t * t + 0.75f;
        //    }
        //    else if (t < (2.5f / 2.75f))
        //    {
        //        t -= (2.5f / 2.75f);
        //        return 7.5625f * t * t + 0.9375f;
        //    }
        //    else
        //    {
        //        t -= (2.625f / 2.75f);
        //        return 7.5625f * t * t + 0.984375f;
        //    }
        //}


        //public static float easeInOut(float t)
        //{
        //    if (t <= 0.5f)
        //        return Bounce.easeIn(t * 2) / 2;
        //    else
        //        return (Bounce.easeOut((t - 0.5f) * 2.0f) / 2) + 0.5f;
        //}
    }


    public static class Elastic
    {
        public static float easeIn(float k)
        {
            if (k == 0) return 0;
            if (k == 1) return 1;
            return -Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
        }

        public static float easeOut(float k)
        {
            if (k == 0) return 0;
            if (k == 1) return 1;
            return Mathf.Pow(2f, -10f * k) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) + 1f;
        }

        public static float easeInOut(float k)
        {
            if ((k *= 2f) < 1f) return -0.5f * Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
            return Mathf.Pow(2f, -10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) * 0.5f + 1f;
        }
    }

}