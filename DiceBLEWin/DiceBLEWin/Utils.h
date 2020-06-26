#pragma once

#include <string>
#include <vector>
#include <locale>

// Forwards
struct _GUID;
typedef _GUID GUID;
struct _BTH_LE_UUID;
typedef _BTH_LE_UUID BTH_LE_UUID;

namespace BLEUtils
{
	std::string ToNarrow(const wchar_t *s, char def = '?', const std::locale& loc = std::locale());
	std::wstring ToWide(const char *s, const std::locale& loc = std::locale());
	std::string GUIDToString(GUID guid);
	std::string BTHLEGUIDToString(const BTH_LE_UUID& bth_le_uuid);
	BTH_LE_UUID StringToBTHLEUUID(const std::string& guid);
	GUID StringToGUID(const std::string& guid);
	GUID BTHLEGUIDToGUID(const BTH_LE_UUID& bth_le_uuid);
	BTH_LE_UUID GUIDToBTHLEGUID(const GUID& guid);
	std::vector<BTH_LE_UUID> GenerateGUIDList(const char* uuidString);
	std::string Base64Encode(unsigned char const* bytes_to_encode, unsigned int in_len);
	std::string Base64Decode(std::string const& encoded_string);
	BTH_LE_UUID MakeBTHLEUUID(USHORT shortId);
}

bool operator==(const BTH_LE_UUID& a, const BTH_LE_UUID& b);
bool operator!=(const BTH_LE_UUID& a, const BTH_LE_UUID& b);

