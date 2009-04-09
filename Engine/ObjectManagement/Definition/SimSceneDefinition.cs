﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Editing;

namespace Engine
{
    public class SimSceneDefinition : EditInterface
    {
        #region Static

        private static LinkedList<CreateEditInterfaceCommand> createCommands = new LinkedList<CreateEditInterfaceCommand>();
        private static CommandManager createCommandManager = new CommandManager();

        static SimSceneDefinition()
        {
            createCommands.AddLast(new CreateEditInterfaceCommand("createSubSceneDefinition", "Create Sub Scene", "Create a new Sub Scene Definition", new CreateEditInterfaceCommand.CreateSubObject(createSimSubSceneDefinition)));
        }

        /// <summary>
        /// This function will create a new SimSubSceneDefinition.
        /// </summary>
        /// <param name="target">The SimSceneDefinition that will get the SubSceneDefinition.</param>
        /// <param name="callback">A callback to get additional input from the user.</param>
        /// <param name="subCommand">The name of the command to run to create the SimElementManagerDefinition.</param>
        /// <returns>An EditInterface to the newly created SimSubSceneDefinition or null if there was an error.</returns>
        private static EditInterface createSimSubSceneDefinition(Object target, EditUICallback callback, String subCommand)
        {
            String name;
            bool accept = callback.getInputString("Enter a name for the subscene.", out name);
            if (accept)
            {
                SimSceneDefinition scene = (SimSceneDefinition)target;
                while (accept && scene.hasSimElementManagerDefinition(name))
                {
                    accept = callback.getInputString("The given name is already in use. Please provide another.", name, out name);
                }
                if (accept)
                {
                    SimSubSceneDefinition subScene = new SimSubSceneDefinition(name);
                    scene.addSimSubSceneDefinition(subScene);
                    return subScene.getEditInterface();
                }
            }
            return null;
        }

        /// <summary>
        /// Add a command from a subsystem for creating new SimElementManagerDefinitions.
        /// </summary>
        /// <param name="command">The command to add.</param>
        public static void AddCreateSimElementManagerDefinitionCommand(EngineCommand command)
        {
            createCommandManager.addCommand(command);
            CreateEditInterfaceCommand createSubObj = new CreateEditInterfaceCommand(command.Name, command.PrettyName, command.HelpText, new CreateEditInterfaceCommand.CreateSubObject(createSimElementManagerDefinition));
            createSubObj.SubCommand = command.Name;
            createCommands.AddLast(createSubObj);
        }

        /// <summary>
        /// Static callback to create new SimElementManagerDefinitions.
        /// </summary>
        /// <param name="target">The SimSceneDefinition that will get the new SimElementManagerDefinition.</param>
        /// <param name="callback">A callback to get additional input from the user.</param>
        /// <param name="subCommand">The name of the command to run to create the SimElementManagerDefinition.</param>
        /// <returns>An EditInterface to the newly created SimElementManagerDefinition or null if there was an error.</returns>
        private static EditInterface createSimElementManagerDefinition(Object target, EditUICallback callback, String subCommand)
        {
            String name;
            bool accept = callback.getInputString("Enter a name.", out name);
            if (accept)
            {
                SimSceneDefinition scene = (SimSceneDefinition)target;
                while (accept && scene.hasSimElementManagerDefinition(name))
                {
                    accept = callback.getInputString("The given name is already in use. Please provide another.", name, out name);
                }
                if (accept)
                {
                    EngineCommand command = createCommandManager.getCommand(subCommand);
                    SimElementManagerDefinition elementDef = (SimElementManagerDefinition)command.execute(name);
                    scene.addSimElementManagerDefinition(elementDef);
                    return elementDef.getEditInterface();
                }
            }
            return null;
        }

        #endregion Static

        #region Fields

        private Dictionary<String, SimElementManagerDefinition> elementManagers = new Dictionary<String,SimElementManagerDefinition>();
        private Dictionary<String, SimSubSceneDefinition> subSceneDefinitions = new Dictionary<string, SimSubSceneDefinition>();
        LinkedList<EditInterface> editInterfaces = new LinkedList<EditInterface>();

        #endregion Fields

        #region Constructors

        public SimSceneDefinition()
        {

        }

        #endregion Constructors

        #region Functions

        public void addSimElementManagerDefinition(SimElementManagerDefinition def)
        {
            elementManagers.Add(def.Name, def);
            editInterfaces.AddLast(def.getEditInterface());
        }

        public void removeSimElementManagerDefinition(SimElementManagerDefinition def)
        {
            elementManagers.Remove(def.Name);
            editInterfaces.Remove(def.getEditInterface());
        }

        public bool hasSimElementManagerDefinition(String name)
        {
            return elementManagers.ContainsKey(name);
        }

        public void addSimSubSceneDefinition(SimSubSceneDefinition def)
        {
            subSceneDefinitions.Add(def.Name, def);
            editInterfaces.AddLast(def.getEditInterface());
        }

        public void removeSimSubSceneDefinition(SimSubSceneDefinition def)
        {
            subSceneDefinitions.Remove(def.Name);
            editInterfaces.Remove(def.getEditInterface());
        }

        public bool hasSimSubSceneDefinition(String name)
        {
            return subSceneDefinitions.ContainsKey(name);
        }

        #region EditInterface Members

        public string getName()
        {
            return "SimSceneDefiniton";
        }

        public bool hasEditableProperties()
        {
            return false;
        }

        public IEnumerable<EditableProperty> getEditableProperties()
        {
            return null;
        }

        public EditablePropertyInfo getPropertyInfo()
        {
            return null;
        }

        public bool hasSubEditInterfaces()
        {
            return true;
        }

        public IEnumerable<EditInterface> getSubEditInterfaces()
        {
            return editInterfaces;
        }

        public bool hasCreateSubObjectCommands()
        {
            return true;
        }

        public IEnumerable<CreateEditInterfaceCommand> getCreateSubObjectCommands()
        {
            return createCommands;
        }

        public bool hasDestroyObjectCommand()
        {
            return false;
        }

        public DestroyEditInterfaceCommand getDestroyObjectCommand()
        {
            return null;
        }

        public object getCommandTargetObject()
        {
            return this;
        }

        /// <summary>
        /// Determine if this interface can create properties.
        /// </summary>
        /// <returns>True if this interface can create properties.</returns>
        public bool canAddRemoveProperties()
        {
            return false;
        }

        /// <summary>
        /// Get the command that creates new properties.
        /// </summary>
        /// <returns>A CreateEditablePropertyCommand to create properties or null if it does not have one.</returns>
        public CreateEditablePropertyCommand getCreatePropertyCommand()
        {
            return null;
        }

        /// <summary>
        /// Get the command that destroys properties.
        /// </summary>
        /// <returns>A DestroyEditablePropertyCommand to destroy properties or null if it does not have one.</returns>
        public DestroyEditablePropertyCommand getDestroyPropertyCommand()
        {
            return null;
        }

        #endregion

        #endregion Functions
    }
}
