#include "StdAfx.h"
#include "..\include\ZipFile.h"
#include "ZipStream.h"
#include "ZipIOException.h"

using namespace System::IO;

namespace ZipAccess
{

ZipFile::ZipFile(void)
:zzipDir(0)
{
}

ZipFile::~ZipFile(void)
{
	if(zzipDir)
	{
		close();
	}
}

void ZipFile::open(String^ filename)
{
	if (!zzipDir)
        {
            zzip_error_t zzipError;
			std::string mName = convertString(filename);
            zzipDir = zzip_dir_open(mName.c_str(), &zzipError);
			if(zzipError != ZZIP_NO_ERROR)
			{
				System::String^ errorMessage;
				switch (zzipError)
				{
					case ZZIP_OUTOFMEM:
						errorMessage = "Out of memory";
						break;            
					case ZZIP_DIR_OPEN:
					case ZZIP_DIR_STAT: 
					case ZZIP_DIR_SEEK:
					case ZZIP_DIR_READ:
						errorMessage = "Unable to read zip file";
						break;           
					case ZZIP_UNSUPP_COMPR:
						errorMessage = "Unsupported compression format";
						break;            
					case ZZIP_CORRUPTED:
						errorMessage = "Archive corrupted";
						break;            
					default:
						errorMessage = "Unknown ZZIP error number";
						break;            
				};
				throw gcnew ZipIOException("Could not open zip file {0} because of {1}", filename, errorMessage);
			}

            //Read the directories and files out of the zip file
            ZZIP_DIRENT zzipEntry;
            while (zzip_dir_read(zzipDir, &zzipEntry))
            {
				String^ entryName = convertString(zzipEntry.d_name);
				Console::WriteLine(entryName);
			}
        }
}

void ZipFile::close()
{
	if (zzipDir)
    {
        zzip_dir_close(zzipDir);
        zzipDir = 0;
    }
}

ZipStream^ ZipFile::openFile(String^ filename)
{
	std::string cFile = convertString(filename);
	//Get uncompressed size
	ZZIP_STAT zstat;
	zzip_dir_stat(zzipDir, cFile.c_str(), &zstat, ZZIP_CASEINSENSITIVE);

	//Open file
	ZZIP_FILE* zzipFile = zzip_file_open(zzipDir, cFile.c_str(), ZZIP_ONLYZIP | ZZIP_CASELESS);
	return gcnew ZipStream(zzipFile, static_cast<size_t>(zstat.st_size));
}

}