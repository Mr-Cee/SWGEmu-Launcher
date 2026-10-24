// Presentation-only server marker carried by UpdatePVPStatusMessage.
constexpr uint32_t citizenMapMarker=0x40000000u;
static const uint32_t citizenPurple=0xffff55ccu; // native UIColor bytes R,G,B,A
using MapPvpFlags=uint32_t(__thiscall*)(const void*);
using MapTypeColor=const uint32_t*(__cdecl*)(int);
using MapObjectColor=const uint32_t*(__cdecl*)(const void*,bool);
static MapTypeColor nativeMapTypeColor;
static MapObjectColor nativeMapObjectColor;
static bool HasCitizenMapMarker(const void* object){
 if(!Readable(object,sizeof(void*)))return false;
 auto table=*(void* const* const*)object;
 if(!Readable(table,0xa4)||!Readable(table[0xa0/4],1))return false;
 return (((MapPvpFlags)table[0xa0/4])(object)&citizenMapMarker)!=0;
}
static const uint32_t* __cdecl CitizenRadarColor(int type,const void* object){
 // Keep dead and group colors; target brackets use their separate native path.
 if(type==4&&HasCitizenMapMarker(object))return &citizenPurple;
 return nativeMapTypeColor(type);
}
static const uint32_t* __cdecl CitizenOverheadColor(const void* object,bool ignoreTarget){
 const uint32_t* normal=nativeMapObjectColor(object,ignoreTarget);
 if(normal&&*normal==0xffffffffu&&HasCitizenMapMarker(object))return &citizenPurple;
 return normal;
}
static uint32_t __cdecl CitizenRadarVisibility(const void* object){
 if(!Readable(object,0x390))return 1;
 uint32_t bits=*(const uint32_t*)((const BYTE*)object+0x38c)>>8;
 return HasCitizenMapMarker(object)?bits&~1u:bits;
}
// The checked ground-radar frame owns the tangible pointer at EBP-0x10.
static __declspec(naked) void CitizenRadarColorBridge(){__asm{
 push dword ptr [ebp-10h]
 push dword ptr [esp+8]
 call CitizenRadarColor
 add esp,8
 ret
}}
static __declspec(naked) void CitizenRadarVisibilityBridge(){__asm{
 push dword ptr [ebp-10h]
 call CitizenRadarVisibility
 add esp,4
 mov edx,eax
 mov ecx,dword ptr [ebp-10h]
 ret
}}
struct CitizenMapCheck{uintptr_t rva;const char* bytes;size_t count;};
static const CitizenMapCheck citizenMapChecks[]={
 {0xbfe0f6,"\x53\xe8\xc4\x39\x98\xff\x8b\x10\x53\x89\x55\xf4",12},
 {0xbfe0bb,"\x8b\x4d\xf0\x8b\x91\x8c\x03\x00\x00\xc1\xea\x08\xf6\xc2\x01",15},
 {0x2de7ce,"\x6a\x01\x57\xe8\xba\x32\x2a\x00\x0f\xb6\x08",11},
 {0x581ac0,"\x55\x8b\xec\xa0\xdc\x27\x93\x01\x84\xc0",10},
 {0x581a90,"\x55\x8b\xec\xa0\xdc\x27\x93\x01\x84\xc0",10}
};
static BYTE* citizenInstalledBase;
static bool InstallCitizenMapColor(BYTE* base){
 if(citizenInstalledBase==base)return true;
 for(const auto& check:citizenMapChecks)if(memcmp(base+check.rva,check.bytes,check.count))return false;
 nativeMapTypeColor=(MapTypeColor)(base+0x581ac0);nativeMapObjectColor=(MapObjectColor)(base+0x581a90);
 const uintptr_t sites[]={0xbfe0f7,0x2de7d1,0xbfe0bb};
 void* hooks[]={(void*)&CitizenRadarColorBridge,(void*)&CitizenOverheadColor,(void*)&CitizenRadarVisibilityBridge};
 const size_t sizes[]={5,5,12};BYTE original[3][12]{};
 for(int i=0;i<3;++i)memcpy(original[i],base+sites[i],sizes[i]);
 for(int i=0;i<3;++i){BYTE patch[12];memset(patch,0x90,sizeof(patch));patch[0]=0xe8;*(int32_t*)(patch+1)=(int32_t)((BYTE*)hooks[i]-(base+sites[i]+5));
  if(!Patch(base+sites[i],patch,sizes[i])){for(int j=i-1;j>=0;--j)Patch(base+sites[j],original[j],sizes[j]);return false;}
 }
 citizenInstalledBase=base;return true;
}
