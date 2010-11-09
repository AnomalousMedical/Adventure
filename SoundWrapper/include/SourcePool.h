#pragma once

#include <vector>

class Source;

using namespace std;

class _AnomalousExport SourcePool
{
private:
	vector<Source*> sources;

public:
	SourcePool(void);

	~SourcePool(void);

	Source* getPooledSource();
};
