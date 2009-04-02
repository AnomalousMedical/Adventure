//Source
#include "stdafx.h"
#include "PhysActorCollection.h"
#include "PhysActor.h"

namespace PhysXWrapper{

PhysActor^ PhysActorCollection::createWrapper(void* nativeObject, ...array<System::Object^>^ args)
{
	return gcnew PhysActor(static_cast<NxActor*>(nativeObject));
}

PhysActor^ PhysActorCollection::getObject(NxActor* nativeObject)
{
	return getObjectVoid(nativeObject);
}

void PhysActorCollection::destroyObject(NxActor* nativeObject)
{
	destroyObjectVoid(nativeObject);
}

}