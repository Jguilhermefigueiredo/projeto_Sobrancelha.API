🎨 SobrancelhaApp API

API backend desenvolvida em ASP.NET Core (.NET 8) para simulação digital de sobrancelhas, com foco em arquitetura limpa, processamento de imagem e evolução controlada para IA.

Este projeto foi construído com objetivo profissional, priorizando organização de código, desacoplamento, clareza arquitetural e manutenibilidade.

📌 Contexto do Projeto

A SobrancelhaApp API é o backend de uma aplicação voltada ao setor de estética facial, onde profissionais podem:

Cadastrar clientes

Enviar imagens faciais

Simular a remoção e substituição digital de sobrancelhas

Visualizar o resultado final via URL

O sistema foi projetado para funcionar hoje (modo simulação) e evoluir amanhã (IA real), sem refatorações estruturais.

🎯 Objetivos Técnicos

Separar claramente camada HTTP, regras de negócio e infraestrutura

Isolar processamento pesado de imagem em serviços especializados

Garantir consistência geométrica independente da resolução da imagem

Preparar o código para troca futura de motores de IA (OpenCV / Dlib / MediaPipe)

Facilitar manutenção e onboarding de novos desenvolvedores

🏗️ Arquitetura

O projeto segue princípios de Clean Architecture, com uso de Service Pattern, Repository Pattern e um serviço orquestrador para o pipeline de imagem.

SombrancelhaApp.Api
│
├── Controllers        → Camada HTTP (entrada/saída)
├── Application        → Orquestração e casos de uso
├── Domain             → Entidades e regras de negócio
├── Infrastructure     → Persistência e motores técnicos
└── BackgroundServices → Rotinas automáticas

Destaque Arquitetural

O ProcessamentoImagemService atua como Facade/Orquestrador, mantendo os Controllers simples e protegendo o restante da API de mudanças técnicas internas.

🧰 Stack Tecnológica
Área	Tecnologia
Framework	ASP.NET Core (.NET 8)
ORM	Entity Framework Core
Banco de Dados	SQLite
Processamento de Imagem	SixLabors.ImageSharp
Visão Computacional	OpenCvSharp4
IA Facial (planejada)	Dlib / MediaPipe
Serialização	System.Text.Json
⚙️ Funcionalidades Implementadas
Gestão de Clientes

Cadastro

Consulta

Atualização

Gestão de Imagens

Upload via multipart/form-data

Persistência física em disco

Associação cliente ↔ imagem

Simulação de Sobrancelhas

Normalização de imagem (512x512)

Remoção digital de pelos (Inpainting – OpenCV)

Aplicação geométrica de moldes gráficos

Ajuste de cor via hexadecimal

Retorno da imagem final por URL

Métricas e Histórico

Registro de atendimentos

Estatísticas para dashboard administrativo

🔗 Endpoints Principais
Clientes
POST   /api/clientes
GET    /api/clientes/{id}
PUT    /api/clientes/{id}

Imagens
POST   /api/clientes/{id}/imagem

Simulação
GET    /api/simulacao/moldes
POST   /api/simulacao/processar
PATCH  /api/simulacao/confirmar-limpeza/{id}

Dashboard
GET    /api/dashboard/estatisticas

🧪 Testes e Validações Técnicas

Testes geométricos (inclinação, escala e espelhamento)

Testes de concorrência (processamentos simultâneos)

Validação visual do Inpainting

Testes de acesso às URLs públicas das imagens

⚠️ Pontos Técnicos Relevantes (Avaliação)

Uso explícito de IDisposable para evitar memory leak em processamento de imagem

Dependência de runtimes nativos do OpenCV

Padronização de resolução como contrato geométrico

Uso de Math.Clamp para evitar acesso inválido à memória gráfica

Background Service para limpeza automática de arquivos

Esses pontos foram tratados de forma consciente para estabilidade em produção.

🧹 Manutenção Automática

A API executa um Background Service periódico que:

Remove arquivos antigos

Limpa registros marcados

Evita crescimento descontrolado do storage

🛣️ Roadmap Técnico

Substituição do mock por detecção facial real (IA)

Processamento assíncrono (jobs em background)

Armazenamento em nuvem (Azure Blob / S3)

Observabilidade e métricas de performance

Autenticação e autorização

📄 Licença

Projeto desenvolvido para fins profissionais.

Inclui:

Portfólio técnico

Avaliação arquitetural

Demonstração de boas práticas

Evolução para uso comercial

Todos os direitos reservados ao autor.
Uso, cópia ou redistribuição do código requer autorização prévia.

👤 Autor

José Guilherme Figueiredo Cavalcante
Backend Developer • .NET • Arquitetura de Software • Processamento de Imagem

🧠 Nota para Avaliadores Técnicos

Este projeto foi estruturado para demonstrar:

Capacidade de organização arquitetural

Tomada de decisões técnicas conscientes

Preocupação com manutenção, escalabilidade e evolução

Código preparado para crescimento sem retrabalho
