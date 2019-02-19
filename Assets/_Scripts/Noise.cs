using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    //We are working on waves:(NOISE)
    //Amplitude: is the y axis of the wave(ocatve)
    //Frequency: is the x axis of the wave(octave)
    //if we want a next level of details we need to add more layer to the noise wave...we call each layer:
    //OCTAVES --- so we want that each layer add some sort of detail to our result noise map so we use:
    //LACUNARITY: controls increase in frequency of octaves (Default value is around 2 f)
    //each octave in order of ascending will add some detail getting lacunarity a power in order, Example:
    //if lacunarity = 2 than:
    //--- first octave-> Frequency= lacunarity ^ 0   = 1
    //--- second octave->Frequency= lacunarity ^ 1 = 2
    //--- third octave-> Frequency= lacunarity ^ 2 = 4 ------- and go on like this ^ 3 , ^ 4 ...^ n

    //we now introducing another variable call PERSISTANCE: (Default value is 0.5f)
    //Persitance: Controls decrease in amplitude of octaves
    //if Persistence = 0.5 than:
    //--- first octave-> Amplitude= persistence ^ 0   = 1
    //--- second octave->Amplitude= persistence ^ 1 = 0.5
    //--- third octave-> Amplitude= persistence ^ 2 = 0.25 ------- and go on like this ^ 3 , ^ 4 ...^ n

    //We need all this stuff to make a good shape for our terrain!


    //we are creating all our waves for the random good terrain generator just giving to this method some values
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight,int seed, float scale, int octaves, float persistence, float lacunarity,Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        //we want to save our unique noisemap and we need to regenerate that map
        //so we save our seed
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        //-------------------------------------------

        //fix a bug that give exeption of division cant be by 0
        if (scale <= 0)
            scale = 0.0001f;
        //-----------------------------------------
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        //variables to just zoom in on the center and not on the topright corner when we grow the noise scale
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        //-----------------------------------------
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    //PerlinNoise return values between 0,1  soooo we multiply this *2 and than subtract by 1
                    //to make this value go negative too
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseMap[x, y] = perlinValue;

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                //controls
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }
        //normalize this value
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}


