﻿using System.Linq;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// The fault class storing all information about a fault.
    /// </summary>
    public class Fault
    {
        public string Placement;
        public string Component;
        public string Description;
        public string Cause;
        public string Classification;
        public string Type;
        public uint Index;
        public int[] ClassIndexes = { -1, -1, -1 };
        public float UserTime;
        public float MachineTime;

        /// <summary>
        /// The fault class storing all information about a fault.
        /// </summary>
        /// <param name="description">Main description of the fault</param>
        /// <param name="cause">the cause of the file for classificaiton</param>
        public Fault(string description, string cause = null)
        {
            Description = description;
            Cause = cause;
        }

        /// <summary>
        /// This returns a new Fault struct whose values are copied from the current Fault struct. 
        /// </summary>
        /// <returns>A new Fault with copied values</returns>
        public Fault Clone()
        {
            Fault clone = new(Description, Cause)
            {
                Placement = Placement,
                Component = Component,
                Classification = Classification,
                Type = Type,
                Index = Index,
                ClassIndexes = ClassIndexes,
                UserTime = UserTime
            };
            return clone;

        }

        /// <summary>
        /// This returns a new Fault struct whose values are copied from the current Fault struct.
        /// </summary>
        /// <param name="fault">The fault to copy</param>
        /// <returns>A new Fault with copied values</returns>
        public bool Equals(Fault fault)
        {
            if (fault == null) return false;
            if (fault.Component != Component) return false;
            if (fault.Description != Description) return false;
            if (fault.Cause != Cause) return false;
            if (fault.Classification != Classification) return false;
            if (fault.Type != Type) return false;

            return true;
        }

        /// <summary>
        /// The new ToString method for the fault class.
        /// </summary>
        /// <returns>the string of fault</returns>
        public override string ToString()
        {
            return $"{Component}\t{Placement}\t{Description}\t{ClassIndexes[0]}\t{ClassIndexes[1]}\t{ClassIndexes[2]}";
        }

        /// <summary>
        /// This method returns the fault in a format that can be exported to a file.
        /// </summary>
        /// <param name="order">The order number</param>
        /// <param name="user">The user who classified the fault</param>
        /// <param name="date">The date of classification</param>
        /// <returns>The string of the fault</returns>
        public string Export(string order, string user, string date)
        {
            string ordernumber = order.Replace(" ", "");

            if (order.StartsWith("38")) { ordernumber = ordernumber.Remove(0, 2); }

            return $"{ordernumber}\t{Placement}\t{Description}\t{Component}\t{Memory.Classification.OriginalCauses[ClassIndexes[0]]}\t{Memory.Classification.OriginalClassifications[ClassIndexes[0]][ClassIndexes[1]]}\t{Memory.Classification.OriginalTypes[Memory.Classification.ClassificationsPointers[ClassIndexes[0]][ClassIndexes[1]]][ClassIndexes[2]]}\t{user}\t{date}";
        }

        /// <summary>
        /// This void checks if the characters provided are valid for a Netstal placement.
        /// Valid placement is each 2 characters long, starts with 'A' and ends with a number.
        /// </summary>
        /// <param name="placement">Placement provided</param>
        /// <returns>Whether the input is valid netstal placement</returns>
        public static bool IsValidNetstalPlacement(string placement)
        {
            if (string.IsNullOrEmpty(placement) || placement.Length != 2) { return false; }
            if (placement.ToLower().ToCharArray()[0] != 'a') { return false; }
            if (!placement.Substring(1, 1).All(char.IsDigit)) { return false; }

            return true;
        }
    }
}
