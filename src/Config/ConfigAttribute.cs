using System; 
using System.IO; 

namespace WebDavSync.Config
{
    public abstract class ConfigAttribute : Attribute 
    {
        public ConfigAttribute(string errorMessage) => ErroMessage = errorMessage; 
        public string ErroMessage {get;}
        //Validates wheter the given value matches the needed conditions
        //for the Attribute
        public abstract bool Validate(object value); 
    }


    public class BiggerThanZero : ConfigAttribute
    {
        public BiggerThanZero() : base("muss be greater than 0") {}

        public override bool Validate(object value) 
        {
            if (value.GetType().Equals(typeof(int)))
            {
                if (((int)value) > 0 ) return true; 
            } 
            return false; 
        }
    }

    public class ConfigPathAbsolute : ConfigAttribute 
    {
        public ConfigPathAbsolute() : base("is not a valid absolute Path") {}

        public override bool Validate(object value) 
        {
            if (value.GetType().Equals(typeof(string)))
            {
                if (!String.IsNullOrEmpty((string)value)) 
                {
                    return Uri.IsWellFormedUriString((string)value, UriKind.Absolute);
                }
            } 
            return false; 
        }
    }

    public class ConfigPathRelative : ConfigAttribute 
    {
        public ConfigPathRelative() : base("is not a valid relative Path") {}

        public override bool Validate(object value) 
        {
            if (value.GetType().Equals(typeof(string)))
            {
                if (!String.IsNullOrEmpty((string)value)) 
                {
                    return Uri.IsWellFormedUriString((string)value, UriKind.Relative);
                }
            } 
            return false; 
        }
    }
    public class ConfigLocalPathMustExist : ConfigAttribute 
    {
        public ConfigLocalPathMustExist() : base ("is not a valid Path or does not exist") {}

        public override bool Validate(object value) 
        {
            if (value.GetType().Equals(typeof(string)))
            {
                return Uri.IsWellFormedUriString((string)value, UriKind.Relative) && Directory.Exists((string)value); 
            }
            return false; 
        }

    }

    public class ConfigLocalPathCanExist : ConfigAttribute 
    {
        public ConfigLocalPathCanExist() : base ("is not a valid Path") {}

        public override bool Validate(object value) 
        {
            if (value.GetType().Equals(typeof(string)))
            {
                return Uri.IsWellFormedUriString((string)value, UriKind.Relative); 
            }
            return false; 
        }

    }

    public class ConfigNotNullOrEmpty : ConfigAttribute 
    {
        public ConfigNotNullOrEmpty() : base ("cannot be null or empty") {}

        public override bool Validate(object value) 
        {
            if (value.GetType().Equals(typeof(string))) 
            {
                return !String.IsNullOrEmpty((string)value); 
            } else 
            {
                return value == null;    
            }
            
        }
    }
}

