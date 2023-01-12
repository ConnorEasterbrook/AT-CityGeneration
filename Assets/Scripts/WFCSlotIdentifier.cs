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
    public class WFCSlotIdentifier : MonoBehaviour
    {
        private int slotPosition;
        private int moduleNumber;
        private Vector3 chunkSize;

        public int rowPosition;
        public int columnPosition;
        public int heightPosition;
        public bool edgeSlot;
        public bool[] otherChunkNeighbourSlot; // The slot position of the other chunk's neighbour slot if the slot is along the edge of the chunk. The order is: left, right, top, bottom

        // public WFCSlotIdentifier(int _slotPosition, int _moduleNumber)
        // {
        //     slotPosition = _slotPosition;
        //     moduleNumber = _moduleNumber;
        // }

        public void EstablishInformation(int _slotPosition, Vector3Int _chunkSize)
        {
            slotPosition = _slotPosition;
            // moduleNumber = _moduleNumber;
            chunkSize = _chunkSize;

            rowPosition = slotPosition % _chunkSize.x;
            columnPosition = slotPosition / _chunkSize.x;
            heightPosition = slotPosition / (_chunkSize.x * _chunkSize.z);

            // Check if the slot is along the edge of the chunk
            if (rowPosition == 0 || rowPosition == _chunkSize.x - 1 || columnPosition == 0 || columnPosition == _chunkSize.z - 1)
            {
                edgeSlot = true;
                CheckSlotNeighbour();
            }
        }

        public void CheckSlotNeighbour()
        {
            otherChunkNeighbourSlot = new bool[4];

            // If the slot is a corner slot
            if (rowPosition == 0 && columnPosition == 0) // Top left corner
            {
                otherChunkNeighbourSlot[0] = true;
                otherChunkNeighbourSlot[2] = true;
            }
            else if (rowPosition == 0 && columnPosition == chunkSize.z - 1) // Top right corner
            {
                otherChunkNeighbourSlot[1] = true;
                otherChunkNeighbourSlot[2] = true;
            }
            else if (rowPosition == chunkSize.x - 1 && columnPosition == 0) // Bottom left corner
            {
                otherChunkNeighbourSlot[0] = true;
                otherChunkNeighbourSlot[3] = true;
            }
            else if (rowPosition == chunkSize.x - 1 && columnPosition == chunkSize.z - 1) // Bottom right corner
            {
                otherChunkNeighbourSlot[1] = true;
                otherChunkNeighbourSlot[3] = true;
            }
            else
            {
                // Check what side the neighbour chunk slot is on
                if (rowPosition == 0) // Far top Side
                {
                    otherChunkNeighbourSlot[2] = true;
                }
                else if (rowPosition == chunkSize.x - 1) // Far bottom side
                {
                    otherChunkNeighbourSlot[3] = true;
                }
                else if (columnPosition == 0) // Far left side
                {
                    otherChunkNeighbourSlot[0] = true;
                }
                else if (columnPosition == chunkSize.z - 1) // Far right side
                {
                    otherChunkNeighbourSlot[1] = true;
                }
            }
        }

        public void CopyInformation(WFCSlotIdentifier slotID)
        {
            slotPosition = slotID.slotPosition;
            moduleNumber = slotID.moduleNumber;
            chunkSize = slotID.chunkSize;
            rowPosition = slotID.rowPosition;
            columnPosition = slotID.columnPosition;
            heightPosition = slotID.heightPosition;
            edgeSlot = slotID.edgeSlot;
            otherChunkNeighbourSlot = slotID.otherChunkNeighbourSlot;
        }
    }
}
