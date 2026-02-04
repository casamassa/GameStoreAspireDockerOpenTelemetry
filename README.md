# GameStore – Observabilidade com Aspire e Docker + OpenTelemetry

Este repositório demonstra **duas formas de executar o mesmo projeto .NET com observabilidade completa**:

1. **Usando .NET Aspire** (modo mais simples e produtivo)

2. **Usando Docker Compose + OpenTelemetry** (modo explícito e controlado)

A ideia aqui não é escolher um “melhor”, mas **comparar abordagens**, entender **o que o Aspire abstrai** e como montar a **mesma stack manualmente** quando Aspire não está disponível.

---

## 🎯 Objetivo do Projeto

- Demonstrar **tracing distribuído**, **métricas** e **logs** em uma aplicação .NET
- Comparar **Aspire vs Docker Compose** no mesmo código-base
- Servir como **referência prática** de OpenTelemetry no mundo real

- Mostrar uma stack moderna de observabilidade usada em produção

---

## 🧰 Tecnologias Utilizadas

### Aplicação

- **.NET 10**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **PostgreSQL**
- **Redis**

### Observabilidade

- **OpenTelemetry** (instrumentação e exportação)
- **Grafana** (visualização)
- **Grafana Tempo** (Tracing distribuído)
- **Prometheus** (Métricas) 🚧
- **Grafana Loki** (Logs) 🚧

> ⚠️ **Nota**: atualmente o projeto está com **Tracing totalmente funcional**.

> As partes de **métricas (Prometheus)** e **logs (Loki)** já estão documentadas e preparadas na stack, mas ainda não instrumentadas na aplicação.

---

## 🧠 Aspire vs Docker Compose (Resumo Rápido)

| Aspecto         | Aspire           | Docker + OTEL              |
| --------------- | ---------------- | -------------------------- |
| Setup inicial   | 🔥 Muito simples | 🧩 Mais verboso            |
| Observabilidade | Automática       | Manual                     |
| Controle fino   | Médio            | Alto                       |
| Infra explícita | Não              | Sim                        |
| Ideal para      | Dev / POC        | Produção / Estudo profundo |

---

## 🚀 Rodando o Projeto com .NET Aspire

Cetifique-se de que o arquivo GameStore.Api.csproj, contenha a seguinte diretiva:

```bash
<PropertyGroup>
    ...
    <DefineConstants>ASPIRE</DefineConstants>
</PropertyGroup>
```

### Pré-requisitos

- .NET SDK 10
- Docker
- .NET Aspire workload instalado

### Passos

```bash
dotnet workload install aspire (Execute apenas senão tiver o Aspire instalado)

dotnet run --project GameStore.AppHost
```

O Aspire irá:

- Subir automaticamente os serviços
- Configurar observabilidade (traces, metrics e logs)
- Disponibilizar o **Aspire Dashboard**

📌 **Nenhuma configuração manual de OpenTelemetry é necessária nesse modo**.

---

## 🐳 Rodando o Projeto com Docker Compose + OpenTelemetry

Cetifique-se de que o arquivo GameStore.Api.csproj, NÂO contenha a seguinte diretiva, se tiver remova:

```bash
<PropertyGroup>
    ...
    <DefineConstants>ASPIRE</DefineConstants>
</PropertyGroup>
```

Neste modo, toda a observabilidade é configurada **explicitamente**, sem Aspire.

### 📌 Passo 0 – Criar a network (executar apenas uma vez)

Este projeto usa uma **network Docker externa compartilhada** entre os composes.

```bash
docker network create observability
```

> ⚠️ Esse comando precisa ser executado **apenas uma vez**.

---

### 📊 Subindo a Stack de Observabilidade

Entre no diretório de observabilidade:

```bash
cd docker/observability

docker compose up -d
```

Serviços que serão iniciados:

- Grafana → http://localhost:3000
- Tempo → http://localhost:3200
- OpenTelemetry Collector
- Prometheus
- Loki

---

### 🧪 Subindo a API

Em outro terminal:

```bash
cd docker/api

docker compose up -d --build
```

A API irá:

- Conectar no PostgreSQL e Redis
- Enviar traces para o **OpenTelemetry Collector**
- Exportar traces para o **Grafana Tempo**

---

## 📈 Visualizando os Traces no Grafana

1. Acesse: http://localhost:3000
2. Vá em **Explore**
3. Selecione o datasource **Tempo**
4. Clique em **Run Query** (ou filtre por "service.name")

Você deverá ver os traces das chamadas da API.

---

## 🛠️ Comandos Úteis do Docker

### Subir um compose com build

```bash
docker compose up -d --build
```

### Derrubar containers e volumes

```bash
docker compose down -v
```

### Ver containers em execução

```bash
docker ps
```

### Ver logs de um serviço específico

```bash
docker compose logs api
```

### Seguir logs em tempo real

```bash
docker compose logs -f otel-collector
```

### Parar tudo rapidamente

```bash
docker stop $(docker ps -q)
```

---

## 📌 Estrutura do Repositório (Simplificada)

```text
/
├── GameStore.Api
├── docker/
│ ├── api/
│ │ └── docker-compose.yml
│ └── observability/
│ ├── docker-compose.yml
│ ├── otel-collector.yml
│ └── tempo.yml
└── README.md
```

---

## 🧭 Próximos Passos (Roadmap)

- [ ] Instrumentar **logs com OpenTelemetry + Loki**
- [ ] Instrumentar **métricas com Prometheus**
- [ ] Correlação Logs ↔ Traces
- [ ] Dashboards customizados no Grafana
- [ ] Comparação de custo e complexidade Aspire vs Docker

---

## 🧠 Conclusão

Este projeto existe para mostrar que:

- Aspire acelera muito o desenvolvimento
- Docker + OpenTelemetry dão controle total
