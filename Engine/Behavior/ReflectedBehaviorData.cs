﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Saving;
using Engine.Editing;

namespace Engine
{
    public class ReflectedBehaviorData : BehaviorData
    {
        private Behavior behaviorTemplate;
        private EditInterface editInterface;

        public ReflectedBehaviorData(Behavior behaviorTemplate)
        {
            this.behaviorTemplate = behaviorTemplate;
        }

        /// <summary>
        /// Get an EditInterface for the Behavior.
        /// </summary>
        /// <returns>The EditInterface for the behavior.</returns>
        public EditInterface getEditInterface()
        {
            if (editInterface == null)
            {
                editInterface = ReflectedEditInterface.createEditInterface(behaviorTemplate, BehaviorEditMemberScanner.Scanner, behaviorTemplate.GetType().Name, null);
            }
            return editInterface;
        }

        /// <summary>
        /// Create a new instance of the Behavior provided by this data.
        /// </summary>
        /// <returns>A new Behavior for the given data.</returns>
        public Behavior createNewInstance()
        {
            //temporary
            return behaviorTemplate;
        }

        #region Saveable Members

        private const String NAME_FORMAT = "{0}, {1}";
        private String BEHAVIOR_TYPE = "BehaviorDataType";

        private ReflectedBehaviorData(LoadInfo info)
        {
            String behaviorType = info.GetString(BEHAVIOR_TYPE);
            Type type = Type.GetType(behaviorType);
            behaviorTemplate = (Behavior)Activator.CreateInstance(type);
            ReflectedSaver.RestoreObject(behaviorTemplate, info, BehaviorSaveMemberScanner.Scanner);
        }

        public void getInfo(SaveInfo info)
        {
            info.AddValue(BEHAVIOR_TYPE, createShortTypeString(behaviorTemplate.GetType()));
            ReflectedSaver.SaveObject(behaviorTemplate, info, BehaviorSaveMemberScanner.Scanner);
        }

        private static String createShortTypeString(Type type)
        {
            String shortAssemblyName = type.Assembly.FullName;
            return String.Format(NAME_FORMAT, type.FullName, shortAssemblyName.Remove(shortAssemblyName.IndexOf(',')));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the
        /// </summary>
        /// <value></value>
        public string Name
        {
            get
            {
                return behaviorTemplate.Name;
            }
        }

        #endregion
    }
}
