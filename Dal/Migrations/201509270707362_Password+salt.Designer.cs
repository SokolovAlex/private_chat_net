// <auto-generated />
namespace Dal.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.1.3-40302")]
    public sealed partial class Passwordsalt : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(Passwordsalt));
        
        string IMigrationMetadata.Id
        {
            get { return "201509270707362_Password+salt"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}
