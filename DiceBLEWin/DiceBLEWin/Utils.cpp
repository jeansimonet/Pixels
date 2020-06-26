
#include "stdafx.h"
#include <windows.h>
#include "Utils.h"

#include <devguid.h>
#include <regstr.h>
#include <bthdef.h>
#include <bluetoothleapis.h>

#include <sstream>
#include <array>

// --------------------------------------------------------------------------
// Helper utilities

std::string BLEUtils::ToNarrow(const wchar_t *s, char def, const std::locale& loc)
{
	std::ostringstream stm;
	while (*s != L'\0')
	{
		stm << std::use_facet< std::ctype<wchar_t> >(loc).narrow(*s++, def);
	}
	return stm.str();
}

std::wstring BLEUtils::ToWide(const char *s, const std::locale& loc)
{
	std::wostringstream stm;
	while (*s != L'\0')
	{
		stm << std::use_facet< std::ctype<char> >(loc).widen(*s++);
	}
	return stm.str();
}

std::string BLEUtils::GUIDToString(GUID guid)
{
	std::array<char, 40> output;
	snprintf(output.data(), output.size(), "%08X-%04hX-%04hX-%02X%02X-%02X%02X%02X%02X%02X%02X", guid.Data1, guid.Data2, guid.Data3, guid.Data4[0], guid.Data4[1], guid.Data4[2], guid.Data4[3], guid.Data4[4], guid.Data4[5], guid.Data4[6], guid.Data4[7]);
	return std::string(output.data());
}

GUID BLEUtils::StringToGUID(const std::string& guid)
{
	GUID output;
	std::wstring wguid;
	bool startsWithBracket = guid[0] == '{';
	if (!startsWithBracket)
	{
		wguid.append(L"{");
	}
	std::copy(guid.begin(), guid.end(), std::back_inserter(wguid));
	if (!startsWithBracket)
	{
		wguid.append(L"}");
	}
	CLSIDFromString(wguid.c_str(), &output);
	return output;
}

GUID BLEUtils::BTHLEGUIDToGUID(const BTH_LE_UUID& bth_le_uuid)
{
	if (bth_le_uuid.IsShortUuid) {
		GUID result = BTH_LE_ATT_BLUETOOTH_BASE_GUID;
		result.Data1 += bth_le_uuid.Value.ShortUuid;
		return result;
	}
	else {
		return bth_le_uuid.Value.LongUuid;
	}
}

BTH_LE_UUID BLEUtils::GUIDToBTHLEGUID(const GUID& guid)
{
	BTH_LE_UUID ret;
	ret.IsShortUuid = true;
	ret.Value.ShortUuid = (unsigned short)guid.Data1;
	return ret;
}


std::string BLEUtils::BTHLEGUIDToString(const BTH_LE_UUID& bth_le_uuid)
{
	if (bth_le_uuid.IsShortUuid)
	{
		std::array<char, 40> output;
		snprintf(output.data(), output.size(), "%04X", bth_le_uuid.Value.ShortUuid);
		return std::string(output.data());
	}
	else
	{
		return GUIDToString(bth_le_uuid.Value.LongUuid);
	}
}

BTH_LE_UUID BLEUtils::StringToBTHLEUUID(const std::string& guid)
{
	BTH_LE_UUID ret;
	GUID fullGUID;
	std::wstring wguid;
	bool startsWithBracket = guid[0] == '{';
	if (!startsWithBracket)
	{
		wguid.append(L"{");
	}
	std::copy(guid.begin(), guid.end(), std::back_inserter(wguid));
	if (!startsWithBracket)
	{
		wguid.append(L"}");
	}
	if (CLSIDFromString(wguid.c_str(), &fullGUID) == S_OK)
	{
		ret.IsShortUuid = false;
		ret.Value.LongUuid = fullGUID;
	}
	else
	{
		ret.IsShortUuid = true;
		ret.Value.ShortUuid = (unsigned short)strtoul(guid.c_str(), NULL, 16);
	}
	return ret;
}

// --------------------------------------------------------------------------
// Parses a string of guid strings and generates a vector of GUID
// --------------------------------------------------------------------------
std::vector<BTH_LE_UUID> BLEUtils::GenerateGUIDList(const char* uuidString)
{
	// Parse serviceUUIDs into an array
	std::stringstream ss(uuidString);
	std::vector<BTH_LE_UUID> uuids;
	while (ss.good())
	{
		std::string substr;
		getline(ss, substr, '|');
		uuids.push_back(BLEUtils::StringToBTHLEUUID(substr));
	}

	return uuids;
}


// Base64 encoding/Decoding copied from here: http://www.cplusplus.com/forum/beginner/51572/

static const std::string base64_chars =
	"ABCDEFGHIJKLMNOPQRSTUVWXYZ"
	"abcdefghijklmnopqrstuvwxyz"
	"0123456789+/";


static inline bool is_base64(unsigned char c) {
	return (isalnum(c) || (c == '+') || (c == '/'));
}

std::string BLEUtils::Base64Encode(unsigned char const* bytes_to_encode, unsigned int in_len) {
	std::string ret;
	int i = 0;
	int j = 0;
	unsigned char char_array_3[3];
	unsigned char char_array_4[4];

	while (in_len--) {
		char_array_3[i++] = *(bytes_to_encode++);
		if (i == 3) {
			char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
			char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
			char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
			char_array_4[3] = char_array_3[2] & 0x3f;

			for (i = 0; (i <4); i++)
				ret += base64_chars[char_array_4[i]];
			i = 0;
		}
	}

	if (i)
	{
		for (j = i; j < 3; j++)
			char_array_3[j] = '\0';

		char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
		char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
		char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
		char_array_4[3] = char_array_3[2] & 0x3f;

		for (j = 0; (j < i + 1); j++)
			ret += base64_chars[char_array_4[j]];

		while ((i++ < 3))
			ret += '=';

	}

	return ret;

}

std::string BLEUtils::Base64Decode(std::string const& encoded_string) {
	size_t in_len = encoded_string.size();
	size_t i = 0;
	size_t j = 0;
	int in_ = 0;
	unsigned char char_array_4[4], char_array_3[3];
	std::string ret;

	while (in_len-- && (encoded_string[in_] != '=') && is_base64(encoded_string[in_])) {
		char_array_4[i++] = encoded_string[in_]; in_++;
		if (i == 4) {
			for (i = 0; i <4; i++)
				char_array_4[i] = static_cast<unsigned char>(base64_chars.find(char_array_4[i]));

			char_array_3[0] = (char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4);
			char_array_3[1] = ((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2);
			char_array_3[2] = ((char_array_4[2] & 0x3) << 6) + char_array_4[3];

			for (i = 0; (i < 3); i++)
				ret += char_array_3[i];
			i = 0;
		}
	}

	if (i) {
		for (j = i; j <4; j++)
			char_array_4[j] = 0;

		for (j = 0; j <4; j++)
			char_array_4[j] = static_cast<unsigned char>(base64_chars.find(char_array_4[j]));

		char_array_3[0] = (char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4);
		char_array_3[1] = ((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2);
		char_array_3[2] = ((char_array_4[2] & 0x3) << 6) + char_array_4[3];

		for (j = 0; (j < i - 1); j++) ret += char_array_3[j];
	}

	return ret;
}

BTH_LE_UUID BLEUtils::MakeBTHLEUUID(USHORT shortId)
{
	BTH_LE_UUID ret;
	ret.IsShortUuid = true;
	ret.Value.ShortUuid = shortId;
	return ret;
}


bool operator==(const BTH_LE_UUID& a, const BTH_LE_UUID& b)
{
	if (a.IsShortUuid != b.IsShortUuid)
	{
		if (a.IsShortUuid)
		{
			GUID longB = b.Value.LongUuid;
			USHORT shortB = (USHORT)longB.Data1;
			longB.Data1 = 0;
			return (longB == BTH_LE_ATT_BLUETOOTH_BASE_GUID && shortB == a.Value.ShortUuid);
		}
		else
		{
			GUID longA = a.Value.LongUuid;
			USHORT shortA = (USHORT)longA.Data1;
			longA.Data1 = 0;
			return (longA == BTH_LE_ATT_BLUETOOTH_BASE_GUID && shortA == b.Value.ShortUuid);
		}
	}
	else
	{
		if (a.IsShortUuid)
			return a.Value.ShortUuid == b.Value.ShortUuid;
		else
			return a.Value.LongUuid == b.Value.LongUuid;
	}
}

bool operator!=(const BTH_LE_UUID& a, const BTH_LE_UUID& b)
{
	return !operator==(a, b);
}