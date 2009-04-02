#pragma once

#include "AutoPtr.h"
#include "Enums.h"
#include "NxPhysics.h"

class NxMeshData;

namespace Physics
{

/// <summary>
/// Descriptor-like user-side class for describing mesh data. 
/// <para>
/// This data type is used for specifying how the SDK is supposed to pass
/// generated mesh data. This is used to pass simulated cloth meshes back to the
/// user.
/// </para>
/// <para>
/// This class is very similar to NxSimpleTriangleMesh, with the difference that
/// this user buffer wrapper is used to let the SDK write to user buffers
/// instead of reading from them. 
/// </para>
/// </summary>
[Engine::Attributes::DoNotSaveAttribute]
public ref class PhysMeshData
{
private:
	AutoPtr<NxMeshData> autoMeshData;

internal:
	NxMeshData* meshData;

	PhysMeshData(NxMeshData* meshData);

	PhysMeshData(const NxMeshData& meshData);

public:
	PhysMeshData();

	~PhysMeshData();

	/// <summary>
	/// Get the NxMeshData this class is wrapping.
	/// </summary>
	/// <returns>The wrapped NxMeshData.</returns>
	NxMeshData* getNxMeshData();

	/// <summary>
	/// (Re)sets the structure to the default.
	/// </summary>
	void setToDefault();

	/// <summary>
	/// Returns true if the current settings are valid.
	/// </summary>
	/// <returns>True if the current settings are valid.</returns>
	bool isValid();

	/// <summary>
	/// The pointer to the user specified buffer for vertex positions. 
	/// <para>
	/// A vertex position consists of three consecutive 32 bit floats. If the
    /// pointer is not initialized (NULL), no data is returned. 
	/// </para>
	/// <para>
	/// If this is being used from a managed environment the buffer must be pinned.
	/// </para>
	/// </summary>
	property void* VerticesPosBegin 
	{
		void* get();
		void set(void* value);
	}

	/// <summary>
	/// The pointer to the user specified buffer for vertex normals. 
	/// <para>
	/// A vertex normal consists of three consecutive 32 bit floats. If the
    /// pointer is not initialized (NULL), no data is returned. 
	/// </para>
	/// <para>
	/// If this is being used from a managed environment the buffer must be
    /// pinned.
	/// </para>
	/// </summary>
	property void* VerticesNormalBegin 
	{
		void* get();
		void set(void* value);
	}

	/// <summary>
	/// Specifies the distance of two vertex position start addresses in bytes.
	/// </summary>
	property System::Int32 VerticesPosByteStride 
	{
		System::Int32 get();
		void set(System::Int32 value);
	}

	/// <summary>
	/// Specifies the distance of two vertex normal start addresses in bytes.
	/// </summary>
	property System::Int32 VerticesNormalByteStride 
	{
		System::Int32 get();
		void set(System::Int32 value);
	}

	/// <summary>
	/// The maximal number of vertices which can be stored in the user vertex buffers.
	/// </summary>
	property System::UInt32 MaxVertices 
	{
		System::UInt32 get();
		void set(System::UInt32 value);
	}

	/// <summary>
	/// Must point to the user allocated memory holding the number of vertices
    /// stored in the user vertex buffers.
	/// </summary>
	property System::UInt32* NumVerticesPtr
	{
		System::UInt32* get();
		void set(System::UInt32* value);
	}

	/// <summary>
	/// The pointer to the user specified buffer for vertex indices. 
	/// <para>
	/// An index consist of one 32 or 16 bit integers, depending on whether
    /// NX_MDF_16_BIT_INDICES has been set.
	/// </para>
	/// <para>
	/// If the pointer is not initialized (NULL), no data is returned. 
	/// </para>
	/// </summary>
	property void* IndicesBegin 
	{
		void* get();
		void set(void* value);
	}

	/// <summary>
	/// Specifies the distance of two vertex indices start addresses in bytes.
	/// </summary>
	property System::Int32 IndicesByteStride 
	{
		System::Int32 get();
		void set(System::Int32 value);
	}

	/// <summary>
	/// The maximal number of indices which can be stored in the user index buffer.
	/// </summary>
	property System::UInt32 MaxIndices 
	{
		System::UInt32 get();
		void set(System::UInt32 value);
	}

	/// <summary>
	/// Must point to the user allocated memory holding the number of indices
    /// stored in the user index buffers.
	/// </summary>
	property System::UInt32* NumIndicesPtr
	{
		System::UInt32* get();
		void set(System::UInt32* value);
	}

	/// <summary>
	/// The pointer to the user specified buffer for vertex parent indices. 
	/// <para>
	/// An index consist of one 32 or 16 bit integers, depending on whether
    /// NX_MDF_16_BIT_INDICES has been set.
	/// </para>
	/// <para>
	/// Parent indices are provided when vertices are duplicated by the SDK
    /// (e.g. cloth tearing). The parent index of an original vertex is its
    /// position in the verticesPos buffer. The parent index of a vertex
    /// generated by duplication is the index of the vertex it was copied from.
	/// </para>
	/// <para>
	/// If the pointer is not initialized (NULL), no data is returned.
	/// </para>
	/// </summary>
	property void* ParentIndicesBegin 
	{
		void* get();
		void set(void* value);
	}

	/// <summary>
	/// Specifies the distance of two vertex parent indices start addresses in
    /// bytes.
	/// </summary>
	property System::Int32 ParentIndicesByteStride 
	{
		System::Int32 get();
		void set(System::Int32 value);
	}

	/// <summary>
	/// The maximal number of parent indices which can be stored in the user
    /// parent index buffer.
	/// </summary>
	property System::UInt32 MaxParentIndices 
	{
		System::UInt32 get();
		void set(System::UInt32 value);
	}

	/// <summary>
	/// Must point to the user allocated memory holding the number of vertex
    /// parent indices. 
	/// </summary>
	property System::UInt32* NumParentIndicesPtr
	{
		System::UInt32* get();
		void set(System::UInt32* value);
	}

	/// <summary>
	/// If the SDK changes the content of a given buffer, it also sets the
    /// corresponding flag of type NxMeshDataDirtyBufferFlags. This
    /// functionality is only supported in conjunction with cloth yet. The
    /// returned value for other features is undefined. Must point to the user
    /// allocated memory holding the dirty buffer flags. 
	/// </summary>
	property System::UInt32* DirtyBufferFlagsPtr 
	{
		System::UInt32* get();
		void set(System::UInt32* value);
	}

	/// <summary>
	/// Flags of type PhysMeshDataFlags.
	/// </summary>
	property PhysMeshDataFlags Flags 
	{
		PhysMeshDataFlags get();
		void set(PhysMeshDataFlags value);
	}
};

}