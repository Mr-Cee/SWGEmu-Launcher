static int junkForward,junkSelects,junkPresses;static long junkSelected,junkCount=3,junkOverride=-999;
static bool junkHitOk=true;static void* junkButton;
static bool __fastcall FakeListProcess(void*,void*,const void*){++junkForward;return false;}
static long __fastcall FakeListCount(void*,void*){return junkCount;}
static bool __fastcall FakeListHit(void* self,void*,const void* point,long* row){
 long y=((const long*)point)[1]+*(long*)((BYTE*)self+0x40);
 *row=junkOverride==-999?(y/20<junkCount?y/20:junkCount-1):junkOverride;return junkHitOk;
}
static void __fastcall FakeListSelect(void*,void*,long row){junkSelected=row;++junkSelects;}
static void* __fastcall FakeFindDefault(void*,void*,bool recurse){assert(recurse);return junkButton;}
static void __fastcall FakePress(void* button,void*){assert(button==junkButton);++junkPresses;}
static void NameWidget(BYTE* widget,const char* name){
 *(const char**)(widget+8)=name;*(const char**)(widget+12)=name+strlen(name);
 *(const char**)(widget+16)=name+strlen(name)+1;
}
static void TestJunkDoubleClick(){
 BYTE* testImage=(BYTE*)VirtualAlloc(nullptr,listSlotRva+64,MEM_COMMIT|MEM_RESERVE,PAGE_READWRITE);assert(testImage);
 for(const auto& check:junkCodeChecks)memcpy(testImage+check.rva,check.bytes,check.size);
 void** slot=(void**)(testImage+listSlotRva);*slot=testImage+listProcessRva;
 slot[-1]=(void*)0x1234;slot[1]=(void*)0x5678;
 assert(InstallJunkDoubleClick(testImage));assert(*slot==(void*)&JunkListProcessHook);
 assert(slot[-1]==(void*)0x1234&&slot[1]==(void*)0x5678);assert(InstallJunkDoubleClick(testImage));
 testImage[junkCodeChecks[1].rva]^=1;assert(!InstallJunkDoubleClick(testImage));
 assert(*slot==(void*)&JunkListProcessHook);testImage[junkCodeChecks[1].rva]^=1;
 *slot=(void*)0x4321;assert(!InstallJunkDoubleClick(testImage));assert(*slot==(void*)0x4321);
 VirtualFree(testImage,0,MEM_RELEASE);
 originalListProcess=(ListProcess)FakeListProcess;nativeListCount=(ListCount)FakeListCount;
 nativeListHit=(ListHit)FakeListHit;nativeListSelect=(ListSelect)FakeListSelect;
 nativeFindDefault=(FindDefault)FakeFindDefault;nativePressButton=(PressButton)FakePress;
 BYTE list[0x150]{},parent[0x150]{},root[0x150]{},ok[0x150]{},style[0x80]{},message[0x28]{};
 NameWidget(list,"lstList");NameWidget(parent,"List");NameWidget(root,"privateJunkDealerSell");NameWidget(ok,"btnOk");
 *(void**)(list+0x14)=parent;*(void**)(parent+0x14)=root;*(void**)(list+0x144)=style;
 *(long*)(style+0x48)=20;*(int*)message=11;*(long*)(message+0x18)=41;
 junkButton=ok;lastJunkPage=nullptr;lastJunkTick=0;
 assert(JunkListProcessHook(list,nullptr,message));assert(junkSelected==2&&junkSelects==1&&junkPresses==1);
 assert(JunkListProcessHook(list,nullptr,message));assert(junkSelects==1&&junkPresses==1);
 *(int*)message=7;assert(!JunkListProcessHook(list,nullptr,message));assert(junkPresses==1);
 *(int*)message=11;NameWidget(root,"listBox");assert(!JunkListProcessHook(list,nullptr,message));assert(junkPresses==1);
 NameWidget(root,"privateJunkDealerSell");NameWidget(list,"otherList");assert(!JunkListProcessHook(list,nullptr,message));
 NameWidget(list,"lstList");lastJunkTick=0;*(long*)(message+0x18)=201;
 assert(!JunkListProcessHook(list,nullptr,message));assert(junkPresses==1);
 *(long*)(message+0x18)=-1;assert(!JunkListProcessHook(list,nullptr,message));
 *(long*)(message+0x18)=1;junkHitOk=false;assert(!JunkListProcessHook(list,nullptr,message));junkHitOk=true;
 junkOverride=8;assert(!JunkListProcessHook(list,nullptr,message));junkOverride=-999;
 junkCount=0;assert(!JunkListProcessHook(list,nullptr,message));junkCount=3;
 junkButton=nullptr;assert(!JunkListProcessHook(list,nullptr,message));junkButton=ok;
 NameWidget(ok,"btnOther");assert(!JunkListProcessHook(list,nullptr,message));assert(junkPresses==1);
 NameWidget(ok,"btnOk");*(long*)(list+0x40)=20;
 assert(JunkListProcessHook(list,nullptr,message));assert(junkSelected==1&&junkPresses==2&&junkSelects==2);
 lastJunkTick=0;*(void**)(parent+0x14)=nullptr;assert(!JunkListProcessHook(list,nullptr,message));assert(junkPresses==2);
 assert(!JunkListProcessHook(nullptr,nullptr,nullptr));assert(junkForward>=10);
 lastJunkPage=nullptr;lastJunkTick=0;
 std::cout<<"PASS: junk list hook selects the clicked/scrolled row and presses only Sell; other windows, single clicks, blank space, invalid rows, missing controls, duplicate input, code/vtable guards and idempotence verified.\n";
}
