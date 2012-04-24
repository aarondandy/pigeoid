// Pigeoid.Interop.Proj4.Unmanaged.h

#pragma once

using namespace System;

namespace Pigeoid { namespace Interop { namespace Proj4 { namespace Unmanaged {

	public ref class Proj4Api sealed abstract
	{
	public:
		static int Transform(
			System::String^ src, System::String^ dst,
			long point_count, int point_offset,
			double %x, double %y, double %z
		);
		static int Transform(
			System::String^ src, System::String^ dst,
			long point_count, int point_offset,
			array<double>^ x, array<double>^ y, array<double>^ z
		);

	};

}}}}
