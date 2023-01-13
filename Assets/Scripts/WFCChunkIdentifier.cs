/**
 * Copyright 2022 Connor Easterbrook
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFCGenerator
{
    public class WFCChunkIdentifier : MonoBehaviour
    {
        public Vector2 chunkPosition;
        public GameObject[] chunkNeighbours = new GameObject[4];
        public Vector2[] supposedNeighbour = new Vector2[4]; // The supposed neighbour of the chunk. The order is: left, right, top, bottom
        public List<GameObject> gridSlots = new List<GameObject>();

        public void EstablishInformation(Vector2 _chunkPos)
        {
            chunkPosition = _chunkPos;

            // Get the chunk neighbours based on the chunk position from the chunks dictionary in WFCGenerator.cs. The order is: left, right, top, bottom
            // If the chunk does not exist, set the neighbour to null. This is done because we do not need to account for non-existent chunks when generating the chunk.
            chunkNeighbours[0] = WFCGenerator.chunks.ContainsKey(new Vector2(chunkPosition.x - 160, chunkPosition.y)) ? WFCGenerator.chunks[new Vector2(chunkPosition.x - 160, chunkPosition.y)] : null; // Left
            chunkNeighbours[1] = WFCGenerator.chunks.ContainsKey(new Vector2(chunkPosition.x + 160, chunkPosition.y)) ? WFCGenerator.chunks[new Vector2(chunkPosition.x + 160, chunkPosition.y)] : null; //Right
            chunkNeighbours[2] = WFCGenerator.chunks.ContainsKey(new Vector2(chunkPosition.x, chunkPosition.y + 160)) ? WFCGenerator.chunks[new Vector2(chunkPosition.x, chunkPosition.y + 160)] : null; // Top
            chunkNeighbours[3] = WFCGenerator.chunks.ContainsKey(new Vector2(chunkPosition.x, chunkPosition.y - 160)) ? WFCGenerator.chunks[new Vector2(chunkPosition.x, chunkPosition.y - 160)] : null; // Bottom
        }

        public void OnGenerationComplete(List<GameObject> _gridSlots)
        {  
            gridSlots = _gridSlots;
        }
    }
}
