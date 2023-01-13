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
        public int moduleNumber;
        private Vector3 chunkSize;
        private string NAME;

        public int rowPosition;
        public int columnPosition;
        public int heightPosition;
        public bool edgeSlot;
        public bool cornerEdgeSlot;
        public bool[] edgeSide; // The slot position of the other chunk's neighbour slot if the slot is along the edge of the chunk. The order is: left, right, top, bottom
        public int singleEdgeSide;
        public GameObject[] otherChunkOppositeSlot; // The slot position of the other chunk's neighbour slot if the slot is along the edge of the chunk. The order is: left, right, top, bottom

        public void EstablishInformation(int _slotPosition, Vector3Int _chunkSize)
        {
            slotPosition = _slotPosition;
            chunkSize = _chunkSize;

            rowPosition = slotPosition % _chunkSize.x;
            columnPosition = slotPosition / _chunkSize.x;
            heightPosition = slotPosition / (_chunkSize.x * _chunkSize.z);

            edgeSide = new bool[4];
            otherChunkOppositeSlot = new GameObject[4];

            // Check if the slot is along the edge of the chunk
            if (rowPosition == 0 || rowPosition == _chunkSize.x - 1 || columnPosition == 0 || columnPosition == _chunkSize.z - 1)
            {
                edgeSlot = true;
                CheckSlotNeighbour();
            }
            else
            {
                NAME = "Inner Slot: " + slotPosition;
                gameObject.name = "Inner Slot: " + slotPosition;
            }
        }

        public void CheckSlotNeighbour()
        {
            // If the slot is a corner slot
            if (rowPosition == 0 && columnPosition == 0) // Bottom left corner
            {
                edgeSide[0] = true;
                edgeSide[3] = true;
                cornerEdgeSlot = true;

                NAME = "Bottom Left Corner | Slot:" + slotPosition;
                gameObject.name = NAME;
            }
            else if (rowPosition == 0 && columnPosition == chunkSize.z - 1) // Top left corner
            {
                edgeSide[0] = true;
                edgeSide[2] = true;
                cornerEdgeSlot = true;

                NAME = "Top Left Corner | Slot:" + slotPosition;
                gameObject.name = NAME;
            }
            else if (rowPosition == chunkSize.x - 1 && columnPosition == 0) // Bottom right corner
            {
                edgeSide[1] = true;
                edgeSide[3] = true;
                cornerEdgeSlot = true;

                NAME = "Bottom Right Corner | Slot:" + slotPosition;
                gameObject.name = NAME;
            }
            else if (rowPosition == chunkSize.x - 1 && columnPosition == chunkSize.z - 1) // Top right corner
            {
                edgeSide[1] = true;
                edgeSide[2] = true;
                cornerEdgeSlot = true;

                NAME = "Top Right Corner | Slot:" + slotPosition;
                gameObject.name = NAME;
            }
            else
            {
                // Check what side the neighbour chunk slot is on
                if (rowPosition == 0) // Far left Side
                {
                    edgeSide[0] = true;
                    singleEdgeSide = 0;

                    NAME = "Left Side | Slot:" + slotPosition;
                    gameObject.name = NAME;
                }
                else if (rowPosition == chunkSize.x - 1) // Far right side
                {
                    edgeSide[1] = true;
                    singleEdgeSide = 1;

                    NAME = "Right Side | Slot:" + slotPosition;
                    gameObject.name = NAME;
                }
                else if (columnPosition == 0) // Far bottom side
                {
                    edgeSide[3] = true;
                    singleEdgeSide = 3;

                    NAME = "Bottom Side | Slot:" + slotPosition;
                    gameObject.name = NAME;
                }
                else if (columnPosition == chunkSize.z - 1) // Far top side
                {
                    edgeSide[2] = true;
                    singleEdgeSide = 2;

                    NAME = "Top Side | Slot:" + slotPosition;
                    gameObject.name = NAME;
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
            edgeSide = slotID.edgeSide;
            NAME = slotID.NAME;
            gameObject.name = NAME;
            moduleNumber = slotID.moduleNumber;

            if (gameObject.activeInHierarchy == true && Application.isPlaying)
            {
                StartCoroutine(SpawnObject());
            }
        }

        private IEnumerator SpawnObject()
        {
            Vector3 originalSize = gameObject.transform.localScale;
            gameObject.transform.localScale = Vector3.zero;
            float spawnDuration = 0.5f;
            float currentTime = 0;

            while (currentTime < spawnDuration)
            {
                transform.localScale = Vector3.Lerp(Vector3.zero, originalSize, currentTime / spawnDuration * 1.25f);
                currentTime += Time.deltaTime;
                yield return null;
            }
        }

        public void SetModuleNumber(int _moduleNumber)
        {
            moduleNumber = _moduleNumber;
        }
    }
}
