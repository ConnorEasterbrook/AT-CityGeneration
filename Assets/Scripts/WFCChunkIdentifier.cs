using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFCGenerator
{
    public class WFCChunkIdentifier : MonoBehaviour
    {
        public Vector2 chunkPosition;
        public GameObject[] chunkNeighbours = new GameObject[4];
        public bool hasNeighbour = false;
        public Vector2[] supposedNeighbour = new Vector2[4]; // The supposed neighbour of the chunk. The order is: down, left, up, right
        public List<GameObject> gridSlots = new List<GameObject>();

        public void EstablishInformation(Vector2 _chunkPos, WFCChunk thisChunk)
        {
            chunkPosition = _chunkPos;

            // Get the chunk neighbours based on the chunk position from the chunks dictionary in WFCGenerator.cs. If the chunk does not exist, set the neighbour to null. This is done because we do not need to account for non-existent chunks when generating the chunk.
            chunkNeighbours[0] = WFCGenerator.chunks.ContainsKey(new Vector2(chunkPosition.x, chunkPosition.y - (thisChunk.gridColumnLength * thisChunk.slotSize))) ? WFCGenerator.chunks[new Vector2(chunkPosition.x, chunkPosition.y - (thisChunk.gridColumnLength * thisChunk.slotSize))] : null;
            chunkNeighbours[1] = WFCGenerator.chunks.ContainsKey(new Vector2(chunkPosition.x - (thisChunk.gridRowWidth * thisChunk.slotSize), chunkPosition.y)) ? WFCGenerator.chunks[new Vector2(chunkPosition.x - (thisChunk.gridRowWidth * thisChunk.slotSize), chunkPosition.y)] : null;
            chunkNeighbours[2] = WFCGenerator.chunks.ContainsKey(new Vector2(chunkPosition.x, chunkPosition.y + (thisChunk.gridColumnLength * thisChunk.slotSize))) ? WFCGenerator.chunks[new Vector2(chunkPosition.x, chunkPosition.y + (thisChunk.gridColumnLength * thisChunk.slotSize))] : null;
            chunkNeighbours[3] = WFCGenerator.chunks.ContainsKey(new Vector2(chunkPosition.x + (thisChunk.gridRowWidth * thisChunk.slotSize), chunkPosition.y)) ? WFCGenerator.chunks[new Vector2(chunkPosition.x + (thisChunk.gridRowWidth * thisChunk.slotSize), chunkPosition.y)] : null;

            if (chunkNeighbours[0] != null || chunkNeighbours[1] != null || chunkNeighbours[2] != null || chunkNeighbours[3] != null)
            {
                hasNeighbour = true;
            }
        }

        public void OnGenerationComplete(List<GameObject> _gridSlots)
        {  
            gridSlots = _gridSlots;
        }
    }
}
