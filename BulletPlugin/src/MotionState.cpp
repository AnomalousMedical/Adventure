#include "StdAfx.h"
#include "..\include\MotionState.h"
#include "RigidBody.h"

using namespace Engine;

namespace BulletPlugin
{

#pragma unmanaged

MotionState::MotionState(gcroot<RigidBody^> rigidBody, float* initialTrans, float* initialRot)
:rigidBody(rigidBody)
{
	transform.setIdentity();
	setStartingTransform(initialTrans, initialRot);
}

void MotionState::getWorldTransform (btTransform &worldTrans) const
{
	worldTrans = transform;
}

void MotionState::setWorldTransform (const btTransform &worldTrans)
{
	btQuaternion rot;
	worldTrans.getBasis().getRotation(rot);
	updateRigidBody(&worldTrans.getOrigin().x(), &rot.x());
}

void MotionState::setStartingTransform(float* trans, float* rot)
{
	btVector3& origin = transform.getOrigin();
	origin.setX(trans[0]);
	origin.setY(trans[1]);
	origin.setZ(trans[2]);
	transform.getBasis().setRotation(btQuaternion(rot[0], rot[1], rot[2], rot[3]));
}

#pragma managed

MotionState::~MotionState(void)
{

}

void MotionState::updateRigidBody(const float* trans, const float* rot)
{
	Vector3 translation(trans[0], trans[1], trans[2]);
	Quaternion rotation(rot[0], rot[1], rot[2], rot[3]);
	rigidBody->updatePosition(translation, rotation);
}

}