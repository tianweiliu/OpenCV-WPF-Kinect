using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSPS
{
    public class PersonHistory : Person
    {
        public DateTime timeStamp;
        public PersonHistory(Person person)
        {
            this.id = person.id;
            this.oid = person.oid;
            this.age = person.age;
            this.centroidX = person.centroidX;
            this.centroidY = person.centroidY;
            this.velocityX = person.velocityX;
            this.velocityY = person.velocityY;
            this.depth = person.depth;
            this.boundingRectOriginX = person.boundingRectOriginX;
            this.boundingRectOriginY = person.boundingRectOriginY;
            this.boundingRectSizeWidth = person.boundingRectSizeWidth;
            this.boundingRectSizeHeight = person.boundingRectSizeHeight;
            this.highestX = person.highestX;
            this.highestY = person.highestY;
            this.haarRectX = person.haarRectX;
            this.haarRectY = person.haarRectY;
            this.haarRectWidth = person.haarRectWidth;
            this.haarRectHeight = person.haarRectHeight;
            this.opticalFlowVelocityX = person.opticalFlowVelocityX;
            this.opticalFlowVelocityY = person.opticalFlowVelocityY;
            this.timeStamp = DateTime.Now;
        }
    }
}
