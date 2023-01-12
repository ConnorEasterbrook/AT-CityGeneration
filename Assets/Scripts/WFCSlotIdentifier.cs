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

        public int rowPosition;
        public int columnPosition;
        public int heightPosition;
        public bool edgeSlot;

        // public WFCSlotIdentifier(int _slotPosition, int _moduleNumber)
        // {
        //     slotPosition = _slotPosition;
        //     moduleNumber = _moduleNumber;
        // }

        public void EstablishInformation(int _slotPosition, int _moduleNumber, Vector3Int chunkSize)
        {
            slotPosition = _slotPosition;
            moduleNumber = _moduleNumber;

            rowPosition = slotPosition % chunkSize.x;
            columnPosition = slotPosition / chunkSize.x;
            heightPosition = slotPosition / (chunkSize.x * chunkSize.z);

            // Check if the slot is along the edge of the chunk
            if (rowPosition == 0 || rowPosition == chunkSize.x - 1 || columnPosition == 0 || columnPosition == chunkSize.z - 1)
            {
                edgeSlot = true;
            }
        }

        // public void CopyInformation(WFCSlotIdentifier slotID)
        // {
        //     slotPosition = slotID.slotPosition;
        //     moduleNumber = slotID.moduleNumber;
        //     rowPosition = slotID.rowPosition;
        //     columnPosition = slotID.columnPosition;
        //     heightPosition = slotID.heightPosition;
        //     edgeSlot = slotID.edgeSlot;
        // }
    }
}
