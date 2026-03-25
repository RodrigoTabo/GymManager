* GymManager

Sistema de gestion de gimnasio desarrollado con .NET, enfocado en backend, buenas practicas de arquitectura y simulacion de un entorno real de produccion.

* Descripcion

GymManager es una aplicacion que permite gestionar socios, pagos y asistencias dentro de un gimnasio.
El objetivo principal del proyecto fue aplicar buenas practicas de desarrollo backend, priorizando la mantenibilidad, escalabilidad y observabilidad del sistema.

* Funcionalidades principales

- Gestion de socios (alta, baja logica y reactivacion)
- Gestion de planes (alta, baja logica)
- Registro de pagos con calculo automatico de cobertura
- Registro de asistencias mediante DNI
- Auditoria de intentos de acceso (exitos y fallos)
- Filtros dinamicos de busqueda (DNI, nombre, plan)

* Arquitectura

El proyecto sigue una arquitectura por capas con separacion de responsabilidades:

- API (ASP.NET Core) - exposicion de endpoints HTTP
- Application Services - logica de negocio
- Infrastructure (Entity Framework Core) - acceso a datos
- Client (Blazor) - interfaz de usuario

- Se priorizo mantener la logica desacoplada del frontend y centralizada en los servicios de aplicacion.

* Autenticacion y Autorizacion

- Implementacion basada en Identity
- Uso de JWT para autenticacion
- Relacion de usuarios con multiples sucursales
- Control de acceso a datos mediante SucursalId

* Buenas practicas implementadas

- V alidaciones desacopladas con FluentValidation
- Ejecución manual de validaciones en services (no dependiente del controller)
- Manejo global de errores mediante middleware
- Logging estructurado en eventos criticos
- Soft delete (baja logica de entidades)
- Multi-tenant (filtrado por sucursal)
- Uso de DTOs para desacoplar capas
- Separacion de responsabilidades en metodos pequeños y claros

* Observabilidad

Se implemento logging estructurado utilizando:

- Serilog
- Seq

Se registran eventos clave como:

- Creacion de pagos
- Alta y baja de socios
- Manejo de excepciones en middleware

Esto permite tener trazabilidad y facilitar el debugging en entornos productivos.

* Manejo de errores

- Se implemento un middleware global que:

- Centraliza el manejo de excepciones
- Devuelve respuestas HTTP consistentes
- Registra errores mediante logging

* Testing

- Implementacion inicial de tests unitarios con xUnit
- Enfocados en la logica de negocio de los servicios

* Infraestructura (Docker)

La aplicacion se encuentra completamente containerizada utilizando Docker, simulando un entorno real de produccion:

- API (.NET)
- Cliente (Blazor)
- Base de datos (SQL Server)
- Sistema de logging (Serilog + Seq)

Esto permite levantar todo el sistema de forma consistente y reproducible.

* Tecnologias utilizadas
- .NET / ASP.NET Core
- Entity Framework Core
- Identity Core
- Blazor
- FluentValidation
- Serilog
- Seq
- Docker
- xUnit

* Estado del proyecto

Versión inicial (V1) completa y funcional.

Se priorizo:

- Correcto diseño de arquitectura
- Implementación de buenas prácticas
- Base solida para futuras mejoras

* Objetivo del proyecto

Este proyecto fue desarrollado como parte de mi formación como desarrollador backend, con foco en:

- Construccion de APIs reales
- Aplicacion de buenas practicas
- Simulacion de entornos productivos
- Preparacion para entornos profesionales

* Proximas mejoras

- Ampliacion de cobertura de tests
- Mejoras en logging y monitoreo
- Optimizacion de queries
- Implementacion de nuevas funcionalidades
