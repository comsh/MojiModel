using System.Collections.Generic;

namespace MojiModel {
public static class ModelConvert {
    private static float[][] empty=new float[0][];
    // OBJExporterからもらう前提なので、文法エラーは全然チェックしてない
    public static ModelFile OBJ2Model(string text) {
        int size=text.Length;
        string line="",token="";
        var v=new List<float[]>();
        var vn=new List<float[]>();
        var uv=new List<float[]>();
        var tri=new List<ushort>();
        
        float x,y,z;
        for(int i=0; i<=size; i=Next(text,'\n',i,size,out line)){
            int cols=line.Length;
            if(cols==0 || line[0]=='#') continue;
            if(line[cols-1]=='\r'){ if(cols==1) continue; cols--; }

            // 行頭の命令
            int c=Next(line,' ',0,cols,out token);
            if(token[0]=='v'){
                if(token.Length==1){        //頂点
                    c=Next(line,' ',c,cols,out token);
                    x=float.Parse(token);
                    c=Next(line,' ',c,cols,out token);
                    y=float.Parse(token);
                    c=Next(line,' ',c,cols,out token);
                    z=float.Parse(token);
                    v.Add(new float[]{x,y,z});
                }else if(token[1]=='t'){    //uv
                    c=Next(line,' ',c,cols,out token);
                    x=float.Parse(token);
                    c=Next(line,' ',c,cols,out token);
                    y=float.Parse(token);
                    uv.Add(new float[]{x,y,0});
                }else if(token[1]=='n'){    //法線
                    c=Next(line,' ',c,cols,out token);
                    x=float.Parse(token);
                    c=Next(line,' ',c,cols,out token);
                    y=float.Parse(token);
                    c=Next(line,' ',c,cols,out token);
                    z=float.Parse(token);
                    vn.Add(new float[]{x,y,z});
                }
            }else if(token[0]=='f'){        //triangle
                string no;
                c=Next(line,' ',c,cols,out token);  // 3頂点で三角形１つ
                string[] sa=token.Split('/');  // 4/4/4のような形。現状全部同じ値なので1つだけとる
                tri.Add((ushort)(ushort.Parse(sa[0])-1));
                c=Next(line,' ',c,cols,out token);
                sa=token.Split('/');
                tri.Add((ushort)(ushort.Parse(sa[0])-1));
                c=Next(line,' ',c,cols,out token);
                sa=token.Split('/');
                tri.Add((ushort)(ushort.Parse(sa[0])-1));
            }else if(token[0]=='g'){}        //サブメッシュ的なもの。現状では作られないっぽい
            // 他にも命令はあるけど見ない
        }
        var ret=ModelFileTemplate(v.Count);
        ret.smr.mesh.vertex=v.ToArray();
        ret.smr.mesh.normal=vn.ToArray();
        ret.smr.mesh.uv=uv.ToArray();
        ret.smr.mesh.triangles=new ushort[][]{ tri.ToArray() };
        return ret;
    }
    private static int Next(string txt,char dlmt,int start,int limit,out string str){
        str="";
        int s=start, e=limit<0?txt.Length:limit;
        for(int i=s; i<e; i++) if(txt[i]==dlmt) s++; else break;
        for(int i=s; i<e; i++) if(txt[i]==dlmt){ e=i; break; }
        if(e>s) str=txt.Substring(s,e-s);
        return e+1;
    }
    public static ModelFile ModelFileTemplate(int vcnt){
        var ret=new ModelFile{
            format=1000,
            smname="moji",
            basebone="Bone_center",
            smr=new ModelFile.SMR{
                bones=new List<ModelFile.Transform>{ new ModelFile.Transform("Bone_center",0,0) },
                mesh=new ModelFile.Mesh{
                    bonelist=new List<string>{ "Bone_center"},
                    bindpose=new List<float[]>{ new float[]{ 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 } },
                    tangents=empty
                },
                mate=new ModelFile.Material[1]{
                    new ModelFile.Material{
                        name="moji",
                        shader="CM3D2/Toony_Lighted",
                        defMaterial="CM3D2__Toony_Lighted",
                        props=new List<ModelFile.Prop>{
                            new ModelFile.Prop("_Color",ModelFile.Prop.Type.Color){
                                value=new ModelFile.PropColor(1,1,1,1)
                            },
                            new ModelFile.Prop("_ToonRamp",ModelFile.Prop.Type.Texture){
                                value=new ModelFile.PropTexture{ name="toonBlackA1", dummy="" }
                            },
                            new ModelFile.Prop("_RimColor",ModelFile.Prop.Type.Color){
                                value=new ModelFile.PropColor(1,1,1,1)
                            },
                        }
                    }
                },
                morph=new List<ModelFile.Morph>()
            }
        };
        ret.smr.mesh.weights=new ModelFile.BoneWeight[vcnt];
        for(int i=0; i<vcnt; i++) ret.smr.mesh.weights[i]=new ModelFile.BoneWeight(0,0,0,0,1,0,0,0);
        return ret;
    }
}
}
