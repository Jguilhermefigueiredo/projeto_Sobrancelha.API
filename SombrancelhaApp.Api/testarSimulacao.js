async function testarSimulacao(fotoFile, clienteId, nomeMolde, corHex) {
    const url = "http://localhost:5043/api/Simulacao/processar"; // Ajuste a porta se necessário

    const formData = new FormData();
    formData.append("foto", fotoFile);
    formData.append("clienteId", clienteId);
    formData.append("nomeMolde", nomeMolde);
    formData.append("corHex", corHex);

    console.log(`🚀 Iniciando teste: Molde [${nomeMolde}] - Cor [${corHex}]`);

    try {
        const response = await fetch(url, {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            const data = await response.json();
            console.log("✅ Sucesso!");
            console.log("🔗 URL da Imagem:", data.urlImagemFinal);

            // Abre a imagem automaticamente em uma nova aba para conferência visual
            window.open(data.urlImagemFinal, '_blank');
        } else {
            const erro = await response.text();
            console.error("❌ Erro na API:", erro);
        }
    } catch (error) {
        console.error("❌ Erro de conexão:", error);
    }
}

// --- EXEMPLO DE USO (BATERIA DE TESTES) ---
// 1. Selecione um arquivo de imagem no seu computador
// 2. Use o comando abaixo no console:

/*
const meuArquivo = document.querySelector('input[type="file"]').files[0];

// Teste de Cor Escura
testarSimulacao(meuArquivo, "teste_anatomia", "sobrancelha_grossa", "#261A15");

// Teste de Cor Clara (Loiro)
testarSimulacao(meuArquivo, "teste_anatomia", "sobrancelha_fina", "#B8860B");
*/