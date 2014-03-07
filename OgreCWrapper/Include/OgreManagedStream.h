#pragma once

typedef size_t (*ReadDelegate)(void* buf, size_t count);
typedef size_t (*WriteDelegate)(const void* buf, size_t count);
typedef void (*SkipDelegate)(size_t count);
typedef void (*SeekDelegate)(size_t pos);
typedef size_t (*TellDelegate)();
typedef bool (*EofDelegate)();
typedef void (*CloseDelegate)();
typedef void (*DeletedDelegate)();

class OgreManagedStream : public Ogre::DataStream
{
private:
	ReadDelegate readCb;
	WriteDelegate writeCb;
	SkipDelegate skipCb;
	SeekDelegate seekCb;
	TellDelegate tellCb;
	EofDelegate eofCb;
	CloseDelegate closeCb;
	DeletedDelegate deletedCb;

public:
	OgreManagedStream(String name, size_t size, ReadDelegate read, WriteDelegate write, SkipDelegate skip, SeekDelegate seek, TellDelegate tell, EofDelegate eof, CloseDelegate close, DeletedDelegate deleted);

	virtual ~OgreManagedStream();

	size_t read(void* buf, size_t count)
	{
		return readCb(buf, count);
	}

	virtual size_t write(const void* buf, size_t count)
    {
       return writeCb(buf, count);
    }

	void skip(long count)
	{
		skipCb(count);
	}

	void seek( size_t pos )
	{
		seekCb(pos);
	}

	size_t tell(void) const
	{
		return tellCb();
	}

	bool eof(void) const
	{
		return eofCb();
	}

	void close(void)
	{
		closeCb();
	}
};
