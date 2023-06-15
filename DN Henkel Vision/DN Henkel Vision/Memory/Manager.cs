﻿using DN_Henkel_Vision.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// This class represents all the neccessary variables and methods for managing the global memory of the application.
    /// </summary>
    internal class Manager
    {
        public static List<string> OrdersRegistry = new();
        public static ObservableCollection<string> VisualRegistry = new();

        public static Order Selected = new();

        public static Editor CurrentEditor;

        /// <summary>
        /// This method initializes the global memory of the application.
        /// </summary>
        public static void Initialize()
        {
            Classification.Assign();
            Drive.Validate();

            OrdersRegistry = Drive.LoadRegistry().ToList<string>();
            VisualRegistry = new(OrdersRegistry);
        }

        /// <summary>
        /// Selects the order from the global memory and loads it into the Selected variable.
        /// </summary>
        /// <param name="orderNumber">Number of the order.</param>
        public static void SelectOrder(string orderNumber)
        {
            // TODO: make this method load the order from the file system\
            Selected = new Order() { OrderNumber = orderNumber };
        }

        public static int CreateIndex()
        {
            //NOTE: Working just up to year 2092
            int index = (int)(DateTime.Now - new DateTime(2023, 4, 3)).TotalSeconds;

            if (index > Cache.LastIndex) { Cache.LastIndex = index; return index; }
                
            Cache.LastIndex++;
            return Cache.LastIndex;
        }
    }
}
