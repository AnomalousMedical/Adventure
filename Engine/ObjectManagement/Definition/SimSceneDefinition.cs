﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Editing;

namespace Engine
{
    /// <summary>
    /// This is a definiton class for a SimSubScene.
    /// </summary>
    public class SimSceneDefinition
    {
        #region Fields

        private Dictionary<String, SimElementManagerDefinition> elementManagers = new Dictionary<String,SimElementManagerDefinition>();
        private Dictionary<String, SimSubSceneDefinition> subSceneDefinitions = new Dictionary<string, SimSubSceneDefinition>();
        private EditInterface editInterface;
        
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public SimSceneDefinition()
        {
            
        }

        #endregion Constructors

        #region Functions

        /// <summary>
        /// Add a SimElementManagerDefinition.
        /// </summary>
        /// <param name="def">The definition to add.</param>
        public void addSimElementManagerDefinition(SimElementManagerDefinition def)
        {
            elementManagers.Add(def.Name, def);
        }

        /// <summary>
        /// Remove a SimElementManagerDefinition.
        /// </summary>
        /// <param name="def">The definition to remove.</param>
        public void removeSimElementManagerDefinition(SimElementManagerDefinition def)
        {
            elementManagers.Remove(def.Name);
        }

        /// <summary>
        /// Get a SimElementManagerDefinition specified by name. Will return
        /// null if it cannot be found.
        /// </summary>
        /// <param name="name">The name of the definition to find.</param>
        /// <returns>The matching defintion or null if it cannot be found.</returns>
        public SimElementManagerDefinition getSimElementManagerDefinition(String name)
        {
            if (elementManagers.ContainsKey(name))
            {
                return elementManagers[name];
            }
            return null;
        }

        /// <summary>
        /// Check to see if a SimElementManagerDefinition is part of this
        /// definition.
        /// </summary>
        /// <param name="name">The name of the definition.</param>
        /// <returns>True if the name is one of the definitons for this scene.</returns>
        public bool hasSimElementManagerDefinition(String name)
        {
            return elementManagers.ContainsKey(name);
        }

        /// <summary>
        /// Add a SimSubSceneDefinition.
        /// </summary>
        /// <param name="def">The definition to add.</param>
        public void addSimSubSceneDefinition(SimSubSceneDefinition def)
        {
            subSceneDefinitions.Add(def.Name, def);
        }

        /// <summary>
        /// Remove a SimSubSceneDefinition.
        /// </summary>
        /// <param name="def">The definition to remove.</param>
        public void removeSimSubSceneDefinition(SimSubSceneDefinition def)
        {
            subSceneDefinitions.Remove(def.Name);
        }

        /// <summary>
        /// Check to see if a named definition exists.
        /// </summary>
        /// <param name="name">The name to check for.</param>
        /// <returns>True if the definition is part of this class.</returns>
        public bool hasSimSubSceneDefinition(String name)
        {
            return subSceneDefinitions.ContainsKey(name);
        }

        /// <summary>
        /// Get the EditInterface for this SimSceneDefinition.
        /// </summary>
        /// <returns>The EditInterface for this SimSceneDefinition.</returns>
        public EditInterface getEditInterface()
        {
            if (editInterface == null)
            {
                editInterface = new SimSceneEditInterface(this);
            }
            return editInterface;
        }

        public SimScene createScene()
        {
            SimScene scene = new SimScene();

            return scene;
        }

        #endregion Functions
    }
}
