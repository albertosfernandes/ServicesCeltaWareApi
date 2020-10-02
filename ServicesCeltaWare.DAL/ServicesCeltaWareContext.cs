using Microsoft.EntityFrameworkCore;
using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServicesCeltaWare.DAL
{
    //Primeiro requisito: Extender a classe DbContext para ter acesso a toda funcionalidade do Entity
    public class ServicesCeltaWareContext : DbContext
    {
        //Segundo requisito: Informar quais classes farão parte da persistência
        public DbSet<ModelCustomer> Customer { get; set; }
        public DbSet<ModelProduct> Product { get; set; }
        public DbSet<ModelCustomerProduct> CustomersProducts { get; set; }
        public DbSet<ModelCertificate> Certificates { get; set; }
        public DbSet<ModelUser> Users { get; set; }

        //Terceiro requisito: Configurar o entity, de forma override que vai sobre-escrever o metodo da classe pai com as 
        //minhas opções de projeto, no meu caso estou informando que vou utilizar o provider SqlServer e passando string conexão
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=CeltaManager;User ID=sa;Password=Celta@123");
        //}



        //Pelo fato do meu context não fazer parte do projeto inicial ele não será chamado automaticamente ao iniciar o projeto
        //por isso eu devo configurar o entity na Startup.cs e lá ja passar as configurações que no caso são Provider SqlServer
        //e a string conexão. Ai no construtor (quando instanciar esta classe) já passo as informações de configurações
        public ServicesCeltaWareContext(DbContextOptions<ServicesCeltaWareContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ModelCustomer>().ToTable("customers");
            modelBuilder.Entity<ModelProduct>().ToTable("products");
            modelBuilder.Entity<ModelCustomerProduct>().ToTable("customersproducts");
            modelBuilder.Entity<ModelCertificate>().ToTable("certificates");
            modelBuilder.Entity<ModelUser>().ToTable("users");
            //modelBuilder.Entity<User>().HasKey(u => u.UserId);

        }
    }
}
