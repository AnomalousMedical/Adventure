﻿using BEPUik;
using Engine;
using Engine.ObjectManagement;
using Engine.Platform;
using Engine.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEPUikPlugin
{
    public class BEPUikScene : SimElementManager
    {
        private String name;
        private BEPUIkFactory factory;
        private UpdateTimer timer;
        private BEPUikSceneUpdater updater;
        private BEPUikSolver rootSolver;
        private Dictionary<String, BEPUikSolver> namedSolvers = new Dictionary<string, BEPUikSolver>();

        public BEPUikScene(BEPUikSceneDefinition definition, UpdateTimer timer)
        {
            this.timer = timer;
            this.name = definition.Name;
            factory = new BEPUIkFactory(this);
            updater = new BEPUikSceneUpdater(this);
            timer.addBackgroundUpdateListener("Rendering", updater);
            rootSolver = new BEPUikSolver(definition, "Root");
            namedSolvers.Add(rootSolver.Name, rootSolver);
        }

        public void Dispose()
        {
            timer.removeBackgroundUpdateListener("Rendering", updater);
        }

        public SimElementFactory getFactory()
        {
            return factory;
        }

        internal BEPUIkFactory IkFactory
        {
            get
            {
                return factory;
            }
        }

        public Type getSimElementManagerType()
        {
            return typeof(BEPUikScene);
        }

        public string getName()
        {
            return name;
        }

        public SimElementManagerDefinition createDefinition()
        {
            return new BEPUikSceneDefinition(name)
                {
                    ActiveSetUseAutomass = rootSolver.IKSolver.ActiveSet.UseAutomass,
                    AutoscaleControlImpulses = rootSolver.IKSolver.AutoscaleControlImpulses,
                    AutoscaleControlMaximumForce = rootSolver.IKSolver.AutoscaleControlMaximumForce,
                    TimeStepDuration = rootSolver.IKSolver.TimeStepDuration,
                    ControlIterationCount = rootSolver.IKSolver.ControlIterationCount,
                    FixerIterationCount = rootSolver.IKSolver.FixerIterationCount,
                    VelocitySubiterationCount = rootSolver.IKSolver.VelocitySubiterationCount
                };
        }

        internal void addBone(BEPUikBone bone)
        {
            BEPUikSolver solver;
            if(namedSolvers.TryGetValue(bone.SolverName, out solver))
            {
                solver.addBone(bone);
            }
            else
            {
                SimObjectErrorManager.AddAndLogError(new SimObjectError()
                {
                    Subsystem = BEPUikInterface.PluginName,
                    ElementName = bone.Name,
                    Type = bone.GetType().Name,
                    SimObject = bone.Owner.Name,
                    Message = String.Format("Cannot find an IKSolver named '{0}' Bone not added to scene.", bone.SolverName)
                });
            }
        }

        internal void removeBone(BEPUikBone bone)
        {
            BEPUikSolver solver;
            if (namedSolvers.TryGetValue(bone.SolverName, out solver))
            {
                solver.removeBone(bone);
            }
            else
            {
                SimObjectErrorManager.AddAndLogError(new SimObjectError()
                {
                    Subsystem = BEPUikInterface.PluginName,
                    ElementName = bone.Name,
                    Type = bone.GetType().Name,
                    SimObject = bone.Owner.Name,
                    Message = String.Format("Cannot find an IKSolver named '{0}' Bone not removed from scene.", bone.SolverName)
                });
            }
        }

        internal void addControl(BEPUikControl control)
        {
            BEPUikSolver solver;
            if (namedSolvers.TryGetValue(control.Bone.SolverName, out solver))
            {
                solver.addControl(control);
            }
            else
            {
                SimObjectErrorManager.AddAndLogError(new SimObjectError()
                {
                    Subsystem = BEPUikInterface.PluginName,
                    ElementName = control.Name,
                    Type = control.GetType().Name,
                    SimObject = control.Owner.Name,
                    Message = String.Format("Cannot find an IKSolver named '{0}' Control not added to scene.", control.Bone.SolverName)
                });
            }
        }

        internal void removeControl(BEPUikControl control)
        {
            BEPUikSolver solver;
            if (namedSolvers.TryGetValue(control.Bone.SolverName, out solver))
            {
                solver.removeControl(control);
            }
            else
            {
                SimObjectErrorManager.AddAndLogError(new SimObjectError()
                {
                    Subsystem = BEPUikInterface.PluginName,
                    ElementName = control.Name,
                    Type = control.GetType().Name,
                    SimObject = control.Owner.Name,
                    Message = String.Format("Cannot find an IKSolver named '{0}' Control not added to scene.", control.Bone.SolverName)
                });
            }
        }

        public bool addExternalControl(ExternalControl control)
        {
            BEPUikSolver solver;
            if (namedSolvers.TryGetValue(control.TargetBone.SolverName, out solver))
            {
                solver.addExternalControl(control);
                return true;
            }
            return false;
        }

        public bool removeExternalControl(ExternalControl control)
        {
            BEPUikSolver solver;
            if (control.CurrentSolverName != null && namedSolvers.TryGetValue(control.CurrentSolverName, out solver))
            {
                solver.removeExternalControl(control);
                return true;
            }
            return false;
        }

        //public void solveJoints(List<IKJoint> joints)
        //{
        //    solver.solveJoints(joints);
        //}

        //public List<IKJoint> findAllJointsFrom(BEPUikBone bone)
        //{
        //    DragControl control = new DragControl();
        //    control.TargetBone = bone.IkBone;
        //    List<Control> dummyList = new List<Control>();
        //    dummyList.Add(control);
        //    ActiveSet dummyActiveSet = new ActiveSet();
        //    dummyActiveSet.UpdateActiveSet(dummyList);
        //    return new List<IKJoint>(dummyActiveSet.Joints);
        //}

        internal void backgroundUpdate()
        {
            PerformanceMonitor.start("BEPU IK Background");
            rootSolver.update(false);
            PerformanceMonitor.stop("BEPU IK Background");
        }

        internal void sync()
        {
            rootSolver.sync();
        }

        internal void drawDebug(DebugDrawingSurface drawingSurface, DebugDrawMode drawMode)
        {
            drawingSurface.begin("BepuIkScene", Engine.Renderer.DrawingType.LineList);
            rootSolver.drawDebug(drawingSurface, drawMode);
            drawingSurface.end();
        }
    }
}
