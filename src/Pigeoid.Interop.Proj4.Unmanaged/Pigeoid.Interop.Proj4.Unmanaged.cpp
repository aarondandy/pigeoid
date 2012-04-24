// This is the main DLL file.

#include "stdafx.h"

#include "Pigeoid.Interop.Proj4.Unmanaged.h"
#include "proj4_src/proj_api.h"

int Pigeoid::Interop::Proj4::Unmanaged::Proj4Api::Transform(
	System::String^ src, System::String^ dst,
	long point_count, int point_offset,
	double %x, double %y, double %z
)
{
	char* srcText;
	char* dstText;

	projPJ pjSrc = NULL;
	projPJ pjDst = NULL;
	int result = -1;

	try{
		srcText = (char*)(System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(src).ToPointer());
		try{
			pjSrc = pj_init_plus(srcText);
		}
		finally{
			System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)srcText));
		}

		dstText = (char*)(System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(dst).ToPointer());
		try{
			pjDst = pj_init_plus(dstText);
		}
		finally{
			System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)dstText));
		}

		if(pjSrc && pjDst){
			double tx = x;
			double ty = y;
			double tz = z;
			result = pj_transform(pjSrc,pjDst,point_count,point_offset,&tx,&ty,&tz);
			x = tx;
			y = ty;
			z = tz;
		}
	}
	catch(...){
		result = -2;
	}
	finally{
		if(pjSrc)
			pj_free(pjSrc);
		if(pjDst)
			pj_free(pjDst);
	}

	return result;
}

int Pigeoid::Interop::Proj4::Unmanaged::Proj4Api::Transform(
	System::String^ src, System::String^ dst,
	long point_count, int point_offset,
	array<double>^ x, array<double>^ y, array<double>^ z
)
{
	char* srcText;
	char* dstText;

	projPJ pjSrc = NULL;
	projPJ pjDst = NULL;
	int result = -1;

	try{
		srcText = (char*)(System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(src).ToPointer());
		try{
			pjSrc = pj_init_plus(srcText);
		}
		finally{
			System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)srcText));
		}
		
		dstText = (char*)(System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(dst).ToPointer());
		try{
			pjDst = pj_init_plus(dstText);
		}
		finally{
			System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)dstText));
		}
		if(pjSrc && pjDst){
			pin_ptr<double> tx = &x[0];
			pin_ptr<double> ty = &y[0];
			pin_ptr<double> tz = &z[0];
			result = pj_transform(pjSrc,pjDst,point_count,point_offset,(double*)tx,(double*)ty,(double*)tz);
		}
	}
	catch(...){
		result = -2;
	}
	finally{
		if(pjSrc)
			pj_free(pjSrc);
		if(pjDst)
			pj_free(pjDst);
	}

	return result;
}