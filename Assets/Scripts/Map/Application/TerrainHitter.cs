﻿using System;
using UnityEngine;

namespace Map.Application
{
    /**
    * Sends Ray and checks if one of the collisions was Terrain
    * Info: good practice is to check collider tag, not it's name ;)
    */
    public class TerrainHitter
    {
        public Vector3 Hit ( Ray ray )
        {
            RaycastHit hit;
            RaycastHit[] hits;
            hits = Physics.RaycastAll( ray, Mathf.Infinity );

            for ( int i = 0; i < hits.Length; i++ )
            {
                hit = hits[ i ];

                if ( hit.collider.name == "Terrain" )
                {

                    return hit.point;
                }
            }

            throw new Exception( "No terrain was hit" );
        }
    }
}