using System.Collections.Generic;
using chat.Enum;

namespace chat.Model
{
       public class MessageModel
    {
        public string Id {get;set;}
        public string Receiver {get;set;}
        public string Message {get;set;}        
        public EnTypeMessage Status {get;set;}
        public List<MemberModel> Members{get;set;} = new List<MemberModel>();
    }


    public class MemberModel {
        public MemberModel(string id){
            this.Id = id;
        }
        public string Id {get;set;}
    }
}