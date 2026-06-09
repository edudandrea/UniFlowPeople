import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { finalize, forkJoin } from 'rxjs';
import { AdmissaoPage } from './pages/admissao-page/admissao-page';
import { BeneficiosPage } from './pages/beneficios-page/beneficios-page';
import { CargosPage } from './pages/cargos-page/cargos-page';
import { ColaboradoresPage } from './pages/colaboradores-page/colaboradores-page';
import { ContratosPage } from './pages/contratos-page/contratos-page';
import { DashboardPage } from './pages/dashboard-page/dashboard-page';
import { DemissaoPage } from './pages/demissao-page/demissao-page';
import { DepartamentosPage } from './pages/departamentos-page/departamentos-page';
import { DocumentosPage } from './pages/documentos-page/documentos-page';
import { EmpresasPage } from './pages/empresas-page/empresas-page';
import { EstruturaPage } from './pages/estrutura-page/estrutura-page';
import { EpisAcessosPage } from './pages/epis-acessos-page/epis-acessos-page';
import { EtapasProcessosPage } from './pages/etapas-processos-page/etapas-processos-page';
import { FeriasPage } from './pages/ferias-page/ferias-page';
import { FinanceiroPage } from './pages/financeiro-page/financeiro-page';
import { PontoPage } from './pages/ponto-page/ponto-page';
import { RecrutamentoPage } from './pages/recrutamento-page/recrutamento-page';
import { SolicitacoesPage } from './pages/solicitacoes-page/solicitacoes-page';
import { TreinamentosPage } from './pages/treinamentos-page/treinamentos-page';
import { UsuariosPage } from './pages/usuarios-page/usuarios-page';

type Role = 'SistemaAdmin' | 'EmpresaAdmin' | 'RH' | 'Gestor' | 'Colaborador';
type MessageType = 'success' | 'error' | 'info';

interface UsuarioInfo {
  id: number;
  empresaId?: number | null;
  colaboradorId?: number | null;
  nome: string;
  login: string;
  email: string;
  role: Role;
}

interface AuthResponse {
  token: string;
  expiresAt: string;
  usuario: UsuarioInfo;
}

interface Empresa {
  id?: number;
  razaoSocial: string;
  nomeFantasia: string;
  cnpj: string;
  telefone?: string;
  email?: string;
  endereco?: string;
  cidade?: string;
  estado?: string;
  cep?: string;
  ativo: boolean;
}

interface Contrato {
  id?: number;
  empresaId: number;
  planoId?: number | null;
  plano: string;
  status: string;
  dataInicio: string;
  dataFim?: string;
  limiteColaboradores: number;
  valorMensal: number;
  observacoes?: string;
  empresa?: Empresa;
  planoCadastro?: Plano;
  cobrancas?: Cobranca[];
}

interface Plano {
  id?: number;
  nome: string;
  prazoDias: number;
  limiteColaboradores: number;
  valorCobranca: number;
  status: string;
  observacoes?: string;
}

interface Cobranca {
  id?: number;
  empresaId: number;
  contratoId: number;
  descricao: string;
  valor: number;
  dataGeracao: string;
  dataVencimento: string;
  status: string;
  empresa?: Empresa;
  contrato?: Contrato;
}

interface UsuarioCreate {
  id?: number;
  empresaId?: number;
  colaboradorId?: number;
  nome: string;
  login: string;
  email: string;
  senha: string;
  role: Role;
  ativo: boolean;
}

interface ModuleItem {
  id: string;
  label: string;
  icon: string;
  description: string;
}

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    FormsModule,
    NgxSpinnerModule,
    AdmissaoPage,
    BeneficiosPage,
    CargosPage,
    ColaboradoresPage,
    ContratosPage,
    DashboardPage,
    DemissaoPage,
    DepartamentosPage,
    DocumentosPage,
    EmpresasPage,
    EstruturaPage,
    EpisAcessosPage,
    EtapasProcessosPage,
    FeriasPage,
    FinanceiroPage,
    PontoPage,
    RecrutamentoPage,
    SolicitacoesPage,
    TreinamentosPage,
    UsuariosPage,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  vm = this;
  private readonly http = inject(HttpClient);
  private readonly spinner = inject(NgxSpinnerService);
  private readonly api = this.resolveApiBase();
  private readonly sessionTimeoutMs = 30 * 60 * 1000;
  private inactivityTimeoutId?: ReturnType<typeof setTimeout>;

  token = signal(sessionStorage.getItem('uniflow.token') ?? '');
  usuario = signal<UsuarioInfo | null>(
    JSON.parse(sessionStorage.getItem('uniflow.usuario') ?? 'null') as UsuarioInfo | null,
  );

  activeModule = signal('dashboard');
  loading = signal(false);
  mensagem = signal('');
  mensagemTipo = signal<MessageType>('info');
  systemAdminExists = signal(true);
  userMenuOpen = signal(false);
  cadastroMenuOpen = signal(false);
  peopleMenuOpen = signal(false);
  financeiroMenuOpen = signal(false);
  recrutamentoMenuOpen = signal(false);
  profileModalOpen = signal(false);
  passwordModalOpen = signal(false);
  admissaoProcessoModalOpen = signal(false);
  admissaoDocumentosModalOpen = signal(false);
  admissaoColaboradorModalOpen = signal(false);
  demissaoProcessoModalOpen = signal(false);
  colaboradorModalOpen = signal(false);
  treinamentoModalOpen = signal(false);
  epiModalOpen = signal(false);
  cargoEpiModalOpen = signal(false);
  colaboradorEpiModalOpen = signal(false);
  ferramentaAcessoModalOpen = signal(false);
  colaboradorFerramentaAcessoModalOpen = signal(false);
  etapaProcessoModalOpen = signal(false);
  vagaModalOpen = signal(false);
  candidatoModalOpen = signal(false);
  curriculoModalOpen = signal(false);
  usuarioModalOpen = signal(false);
  documentoModalOpen = signal(false);
  solicitacaoModalOpen = signal(false);
  empresaModalOpen = signal(false);
  planoModalOpen = signal(false);
  contratoModalOpen = signal(false);
  cobrancaModalOpen = signal(false);
  contratoPreviewModalOpen = signal(false);
  contratoAssinatura = signal<'GOV.BR' | 'Certificado digital'>('GOV.BR');
  contratoGerado = signal<Contrato | null>(null);
  genericEditModal = signal<{ form: string; title: string } | null>(null);
  confirmDelete = signal<{ endpoint: string; id: number; label: string } | null>(null);
  cpfColaboradorDuplicado = signal(false);
  search = signal('');
  cargoSearch = signal('');
  departamentoSearch = signal('');
  usuarioSearch = signal('');
  vagaSearch = signal('');
  candidatoSearch = signal('');
  curriculoSearch = signal('');
  etapaProcessoSearch = signal('');
  documentoSearch = signal('');
  solicitacaoSearch = signal('');
  colaboradorSearch = signal('');
  admissaoSearch = signal('');
  demissaoSearch = signal('');
  planoSearch = signal('');
  contratoSearch = signal('');
  cobrancaSearch = signal('');
  admissaoPage = signal(1);
  demissaoPage = signal(1);
  empresasTab = signal<'pesquisa' | 'dados'>('pesquisa');
  usuariosTab = signal<'pesquisa' | 'dados'>('pesquisa');
  cargosTab = signal<'pesquisa' | 'dados'>('pesquisa');
  departamentosTab = signal<'pesquisa' | 'dados'>('pesquisa');
  filiaisTab = signal<'pesquisa' | 'dados'>('pesquisa');
  beneficiosTab = signal<'pesquisa' | 'dados'>('pesquisa');
  beneficiosVinculosTab = signal<'pesquisa' | 'dados'>('pesquisa');
  colaboradoresTab = signal<'pesquisa' | 'dados'>('pesquisa');
  planosTab = signal<'pesquisa' | 'dados'>('pesquisa');
  contratacoesTab = signal<'pesquisa' | 'dados'>('pesquisa');
  cobrancasTab = signal<'pesquisa' | 'dados'>('pesquisa');
  recrutamentoCadastro = signal<'vagas' | 'candidatos' | 'curriculos'>('vagas');
  recrutamentoTab = signal<'pesquisa' | 'dados'>('pesquisa');
  empresaLogada = signal<Empresa | null>(JSON.parse(sessionStorage.getItem('uniflow.empresa') ?? 'null') as Empresa | null);
  profilePhotoUrl = signal(localStorage.getItem('uniflow.profilePhoto') ?? '');

  loginForm = { login: '', senha: '' };
  bootstrapForm = {
    nome: 'Administrador do Sistema',
    login: 'admin',
    email: 'admin@uniflowpeople.com',
    senha: 'Admin@123',
  };

  profileForm = { nome: '', email: '' };
  passwordForm = { senhaAtual: '', novaSenha: '', confirmarSenha: '' };

  empresaForm: Empresa = this.novaEmpresa();
  planoForm: Plano = this.novoPlano();
  contratoForm: Contrato = this.novoContrato();
  cobrancaForm: Cobranca = this.novaCobranca();
  usuarioForm: UsuarioCreate = this.novoUsuario();
  filialForm: any = this.novaFilial();
  departamentoForm: any = this.novoDepartamento();
  cargoForm: any = this.novoCargo();
  colaboradorForm: any = this.novoColaborador();
  beneficioForm: any = this.novoBeneficio();
  vagaForm: any = this.novaVaga();
  candidatoForm: any = this.novoCandidato();
  curriculoForm: any = this.novoCurriculo();
  curriculoArquivo: File | null = null;
  treinamentoForm: any = this.novoTreinamento();
  treinamentoColaboradorForm: any = this.novoTreinamentoColaborador();
  epiForm: any = this.novoEpi();
  cargoEpiForm: any = this.novoCargoEpi();
  colaboradorEpiForm: any = this.novoColaboradorEpi();
  ferramentaAcessoForm: any = this.novaFerramentaAcesso();
  colaboradorFerramentaAcessoForm: any = this.novoColaboradorFerramentaAcesso();
  etapaProcessoForm: any = this.novaEtapaProcesso();
  admissaoForm: any = this.novaAdmissao();
  admissaoSelecionada: any = null;
  admissaoEtapaSelecionada = signal<{ processoId: number; etapaNome: string } | null>(null);
  admissaoDocumentosAnexados = signal<Record<number, any[]>>({});
  demissaoForm: any = this.novaDemissao();
  demissaoEtapaSelecionada = signal<{ processoId: number; etapaNome: string } | null>(null);
  feriasForm: any = this.novaFerias();
  pontoForm: any = this.novoPonto();
  documentoForm: any = this.novoDocumento();
  beneficioColaboradorForm: any = this.novoBeneficioColaborador();
  solicitacaoForm: any = this.novaSolicitacao();

  empresas = signal<Empresa[]>([]);
  planos = signal<Plano[]>([]);
  contratos = signal<Contrato[]>([]);
  cobrancas = signal<Cobranca[]>([]);
  contratoAtivoEmpresa = signal<Contrato | null>(null);
  cobrancasEmpresa = signal<Cobranca[]>([]);
  usuarios = signal<UsuarioInfo[]>([]);
  filiais = signal<any[]>([]);
  departamentos = signal<any[]>([]);
  cargos = signal<any[]>([]);
  colaboradores = signal<any[]>([]);
  beneficios = signal<any[]>([]);
  vagas = signal<any[]>([]);
  candidatos = signal<any[]>([]);
  curriculos = signal<any[]>([]);
  treinamentos = signal<any[]>([]);
  treinamentosColaboradores = signal<any[]>([]);
  epis = signal<any[]>([]);
  cargosEpis = signal<any[]>([]);
  colaboradoresEpis = signal<any[]>([]);
  ferramentasAcesso = signal<any[]>([]);
  colaboradoresFerramentasAcesso = signal<any[]>([]);
  etapasProcessosConfig = signal<any[]>([]);
  admissoes = signal<any[]>([]);
  demissoes = signal<any[]>([]);
  documentosInstitucionais = signal<any[]>([]);
  ferias = signal<any[]>([]);
  pontos = signal<any[]>([]);
  documentos = signal<any[]>([]);
  beneficiosColaboradores = signal<any[]>([]);
  solicitacoes = signal<any[]>([]);

  modelosDocumentosAdmissao = [
    {
      titulo: 'Pausas para café',
      arquivo: 'TERMO DE CIÊNCIA CONCORDÂNCIA E ADESÃO À POLÍTICA INTERNA DE PAUSAS PARA CAFÉ.docx',
      tipo: 'DOCX',
      template: 'pausas-cafe.txt',
    },
    {
      titulo: 'Chaves e senhas de alarmes',
      arquivo: 'Termo de Responsabilidade de Chaves e Senhas de Alarmes kONECT.doc',
      tipo: 'DOC',
      template: 'chaves-senhas-alarmes.txt',
    },
    {
      titulo: 'Refeição KONECT',
      arquivo: 'Termo ciência REFEICAO KONECT.docx',
      tipo: 'DOCX',
      template: 'refeicao-konect.txt',
    },
    {
      titulo: 'Alelo Alimentação e absenteísmo',
      arquivo: 'Alelo Alimentação - Termo Recebimento + Absenteismo KONECT (Reparado).doc',
      tipo: 'DOC',
      template: 'alelo-alimentacao-absenteismo.txt',
    },
    {
      titulo: 'Celulares',
      arquivo: 'TERMO CELULARES.docx',
      tipo: 'DOCX',
      template: 'celulares.txt',
    },
    {
      titulo: 'Imagem e LGPD',
      arquivo: 'ADITIVO CONTRATO DE TRABALHO - KONECT- IMAGEM LGPD (1) (1).docx',
      tipo: 'DOCX',
      template: 'imagem-lgpd.txt',
    },
  ];

  metricas = signal({
    empresasContratantes: 0,
    pagamentosEfetuados: 0,
    pagamentosPendentes: 0,
    contratosAVencer: 0,
    contratosVencidos: 0,
  });

  isLogged = computed(() => !!this.token());
  isSistemaAdmin = computed(() => this.usuario()?.role === 'SistemaAdmin');
  userName = computed(() => this.usuario()?.nome ?? '');
  topbarEyebrow = computed(() => {
    if (this.isSistemaAdmin()) return 'Backoffice SaaS';
    const empresa = this.empresaLogada();
    return empresa?.nomeFantasia || empresa?.razaoSocial || this.usuario()?.role || 'Portal da Empresa';
  });
  userInitials = computed(() =>
    (this.usuario()?.nome ?? 'UF')
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((x) => x[0])
      .join('')
      .toUpperCase(),
  );

  pageTitle = computed(
    () =>
      this.modules().find((x) => x.id === this.activeModule())?.label ??
      this.cadastroModules().find((x) => x.id === this.activeModule())?.label ??
      this.financeiroModules().find((x) => x.id === this.activeModule())?.label ??
      this.peopleModules().find((x) => x.id === this.activeModule())?.label ??
      this.recrutamentoModules().find((x) => x.id === this.activeModule())?.label ??
      'Dashboard',
  );
  pageDescription = computed(
    () =>
      this.modules().find((x) => x.id === this.activeModule())?.description ??
      this.cadastroModules().find((x) => x.id === this.activeModule())?.description ??
      this.financeiroModules().find((x) => x.id === this.activeModule())?.description ??
      this.peopleModules().find((x) => x.id === this.activeModule())?.description ??
      this.recrutamentoModules().find((x) => x.id === this.activeModule())?.description ??
      'Gestão inteligente de pessoas.',
  );

  modules = computed<ModuleItem[]>(() =>
    this.isSistemaAdmin()
      ? [
          { id: 'dashboard', label: 'Dashboard SaaS', icon: '◈', description: 'Contratos, pagamentos e saúde da carteira.' },
          { id: 'empresas', label: 'Empresas', icon: '▣', description: 'Cadastro e manutenção das empresas contratantes.' },
          { id: 'usuarios', label: 'Usuários', icon: '◎', description: 'Acessos administrativos das empresas.' },
        ]
      : [
          { id: 'dashboard', label: 'Visão geral', icon: '✦', description: 'Indicadores rápidos do RH.' },
          { id: 'documentos', label: 'Documentos', icon: '▧', description: 'Documentos e validações dos colaboradores.' },
          { id: 'solicitacoes', label: 'Solicitações', icon: '▢', description: 'Pedidos enviados ao setor de RH.' },
          { id: 'treinamentos', label: 'Treinamentos', icon: '◎', description: 'Portal de treinamentos, colaboradores e presença.' },
          { id: 'usuarios', label: 'Usuários', icon: '◎', description: 'Contas e perfis de acesso.' },
        ],
  );

  cadastroModules = computed<ModuleItem[]>(() => [
    ...(this.isSistemaAdmin()
      ? [
          { id: 'planos', label: 'Planos', icon: '◧', description: 'Catálogo de planos, prazos e valores.' },
          { id: 'contratacoes', label: 'Contratos', icon: '◷', description: 'Contratos firmados com empresas contratantes.' },
          { id: 'cobrancas', label: 'Cobranças', icon: '▤', description: 'Cobranças geradas e status financeiro.' },
        ]
      : [
          { id: 'cargos', label: 'Cargos', icon: '◧', description: 'Cadastro de funções, níveis e salários base.' },
          { id: 'colaboradores', label: 'Colaboradores', icon: '◉', description: 'Cadastro completo dos colaboradores.' },
          { id: 'epis-acessos', label: 'EPIs e Acessos', icon: '▨', description: 'EPIs por cargo, fichas de retirada e ferramentas de acesso.' },
          { id: 'etapas-processos', label: 'Etapas', icon: '▱', description: 'Etapas personalizadas para admissão e demissão.' },
          { id: 'departamentos', label: 'Departamentos', icon: '▦', description: 'Cadastro de áreas e gestores.' },
          { id: 'estrutura', label: 'Filiais', icon: '▤', description: 'Unidades e filiais da empresa.' },
        ]),
  ]);

  peopleModules = computed<ModuleItem[]>(() => [
    { id: 'admissao', label: 'Admissão', icon: '＋', description: 'Fluxo admissional e documentos institucionais.' },
    { id: 'demissao', label: 'Demissão', icon: '−', description: 'Fluxo demissional e encerramento de vínculo.' },
    { id: 'beneficios', label: 'Benefícios', icon: '◆', description: 'Benefícios e vínculos com colaboradores.' },
    { id: 'ferias', label: 'Férias', icon: '☼', description: 'Programação e controle de férias.' },
    { id: 'ponto', label: 'Ponto', icon: '◔', description: 'Registros de ponto por colaborador.' },
  ]);

  recrutamentoModules = computed<ModuleItem[]>(() => [
    { id: 'recrutamento-vagas', label: 'Vagas', icon: '◇', description: 'Cadastro, publicação e acompanhamento de vagas.' },
    { id: 'recrutamento-candidatos', label: 'Candidatos', icon: '◌', description: 'Pipeline de candidatos vinculados às vagas.' },
    { id: 'recrutamento-curriculos', label: 'Banco de currículos', icon: '▧', description: 'Currículos recebidos e disponíveis para triagem.' },
  ]);

  financeiroModules = computed<ModuleItem[]>(() =>
    this.isSistemaAdmin()
      ? []
      : [
          { id: 'financeiro', label: 'Contrato e cobranças', icon: '▥', description: 'Plano ativo, cobranças geradas e histórico financeiro.' },
        ],
  );

  filteredEmpresas = computed(() => this.filterByTerm(this.empresas(), ['nomeFantasia', 'razaoSocial', 'cnpj']));
  filteredContratos = computed(() => {
    const term = this.contratoSearch().trim().toLowerCase() || this.search().trim().toLowerCase();
    const items = this.contratos();
    if (!term) return items;
    return items.filter((item) =>
      [item.plano, item.status, this.nomeEmpresa(item.empresaId)].some((value) => String(value ?? '').toLowerCase().includes(term)),
    );
  });
  filteredPlanos = computed(() => {
    const term = this.planoSearch().trim().toLowerCase() || this.search().trim().toLowerCase();
    const items = this.planos();
    if (!term) return items;
    return items.filter((item) => [item.nome, item.status].some((value) => String(value ?? '').toLowerCase().includes(term)));
  });
  filteredCobrancas = computed(() => {
    const term = this.cobrancaSearch().trim().toLowerCase() || this.search().trim().toLowerCase();
    const items = this.cobrancas();
    if (!term) return items;
    return items.filter((item) =>
      [item.descricao, item.status, this.nomeEmpresa(item.empresaId), item.valor].some((value) =>
        String(value ?? '').toLowerCase().includes(term),
      ),
    );
  });
  filteredUsuarios = computed(() =>
    this.filterBySignal(this.usuarios(), this.usuarioSearch(), ['nome', 'login', 'email', 'role'], (item) =>
      String(this.nomeEmpresa(item.empresaId || undefined)),
    ),
  );
  filteredCargos = computed(() => this.filterBySignal(this.cargos(), this.cargoSearch(), ['nome', 'nivel', 'descricao']));
  filteredDepartamentos = computed(() =>
    this.filterBySignal(this.departamentos(), this.departamentoSearch(), ['nome', 'descricao'], (item) =>
      this.nomeColaborador(item.gestorId),
    ),
  );
  filteredColaboradores = computed(() => this.filterByTerm(this.colaboradores(), ['nome', 'cpf', 'email', 'status']));
  filteredColaboradoresCadastro = computed(() => {
    const term = this.colaboradorSearch().trim().toLowerCase();
    const items = this.filteredColaboradores();
    if (!term) return items;

    return items.filter((item) => {
      const cargo = this.nomeCargo(item.cargoId);
      return [item.nome, item.cpf, item.telefone, cargo].some((value) =>
        String(value ?? '').toLowerCase().includes(term),
      );
    });
  });
  filteredVagas = computed(() =>
    this.filterBySignal(this.vagas(), this.vagaSearch(), ['titulo', 'status', 'descricao'], (item) =>
      [this.nomeDepartamento(item.departamentoId), this.nomeCargo(item.cargoId)].join(' '),
    ),
  );
  filteredCandidatos = computed(() =>
    this.filterBySignal(this.candidatos(), this.candidatoSearch(), ['nome', 'email', 'telefone', 'status', 'linkedin'], (item) =>
      this.nomeVaga(item.vagaId),
    ),
  );
  filteredCurriculos = computed(() => this.filterByTerm(this.curriculos(), ['nome', 'email', 'telefone', 'status']));
  filteredCurriculosBanco = computed(() => {
    const term = this.curriculoSearch().trim().toLowerCase();
    const items = this.filteredCurriculos();
    if (!term) return items;
    return items.filter((item) =>
      ['nome', 'telefone', 'email'].some((field) => String(item[field] ?? '').toLowerCase().includes(term)),
    );
  });
  filteredAdmissoes = computed(() => {
    const term = this.admissaoSearch().trim().toLowerCase();
    const items = this.admissoes();
    if (!term) return items;

    return items.filter((item) =>
      [item.nomeCandidato, item.email, item.telefone, item.cargoPretendido, this.statusAdmissao(item)].some((value) =>
        String(value ?? '').toLowerCase().includes(term),
      ),
    );
  });
  filteredDemissoes = computed(() => {
    const term = this.demissaoSearch().trim().toLowerCase();
    const items = this.demissoes();
    if (!term) return items;

    return items.filter((item) =>
      [
        this.nomeColaborador(item.colaboradorId),
        item.colaborador?.email,
        item.colaborador?.telefone,
        item.tipoDemissao,
        item.motivo,
        item.observacoes,
        this.statusDemissao(item),
      ].some((value) => String(value ?? '').toLowerCase().includes(term)),
    );
  });
  filteredEtapasProcessos = computed(() =>
    this.filterBySignal(this.etapasProcessosOrdenadas(), this.etapaProcessoSearch(), [
      'tipoProcesso',
      'nome',
      'descricao',
      'ordem',
    ]),
  );
  filteredDocumentos = computed(() =>
    this.filterBySignal(this.documentos(), this.documentoSearch(), ['tipoDocumento', 'nomeArquivo', 'urlArquivo'], (item) =>
      this.nomeColaborador(item.colaboradorId),
    ),
  );
  filteredSolicitacoes = computed(() =>
    this.filterBySignal(this.solicitacoes(), this.solicitacaoSearch(), [
      'tipo',
      'titulo',
      'descricao',
      'prioridade',
      'status',
      'respostaRh',
    ], (item) => this.nomeColaborador(item.colaboradorId)),
  );
  admissaoTotalPages = computed(() => Math.max(1, Math.ceil(this.filteredAdmissoes().length / 10)));
  admissaoCurrentPage = computed(() => Math.min(this.admissaoPage(), this.admissaoTotalPages()));
  pagedAdmissoes = computed(() => {
    const page = this.admissaoCurrentPage();
    const start = (page - 1) * 10;
    return this.filteredAdmissoes().slice(start, start + 10);
  });
  demissaoTotalPages = computed(() => Math.max(1, Math.ceil(this.filteredDemissoes().length / 10)));
  demissaoCurrentPage = computed(() => Math.min(this.demissaoPage(), this.demissaoTotalPages()));
  pagedDemissoes = computed(() => {
    const page = this.demissaoCurrentPage();
    const start = (page - 1) * 10;
    return this.filteredDemissoes().slice(start, start + 10);
  });

  constructor() {
    this.limparSessaoPersistenteAntiga();
    this.registrarMonitorInatividade();
    this.verificarAdminInicial();
    if (this.isLogged()) {
      this.reiniciarTimerInatividade();
      this.carregarDados();
      this.prepararPerfil();
    }
  }

  login() {
    this.setLoading(true);
    this.http
      .post<AuthResponse>(`${this.api}/auth/login`, this.loginForm)
      .pipe(finalize(() => this.setLoading(false)))
      .subscribe({
        next: (response) => this.aplicarLogin(response),
        error: () => this.notificar('Login ou senha inválidos.', 'error'),
      });
  }

  bootstrapAdmin() {
    this.setLoading(true);
    this.http
      .post<AuthResponse>(`${this.api}/auth/bootstrap-system-admin`, this.bootstrapForm)
      .pipe(finalize(() => this.setLoading(false)))
      .subscribe({
        next: (response) => {
          this.systemAdminExists.set(true);
          this.aplicarLogin(response);
        },
        error: () => this.notificar('Admin já criado ou backend indisponível.', 'error'),
      });
  }

  logout() {
    this.pararTimerInatividade();
    localStorage.removeItem('uniflow.token');
    localStorage.removeItem('uniflow.usuario');
    localStorage.removeItem('uniflow.empresa');
    sessionStorage.removeItem('uniflow.token');
    sessionStorage.removeItem('uniflow.usuario');
    sessionStorage.removeItem('uniflow.empresa');
    this.token.set('');
    this.usuario.set(null);
    this.empresaLogada.set(null);
    this.userMenuOpen.set(false);
    this.cadastroMenuOpen.set(false);
    this.peopleMenuOpen.set(false);
    this.financeiroMenuOpen.set(false);
    this.activeModule.set('dashboard');
  }

  abrirPerfil() {
    this.prepararPerfil();
    this.profileModalOpen.set(true);
    this.userMenuOpen.set(false);
  }

  abrirSenha() {
    this.passwordForm = { senhaAtual: '', novaSenha: '', confirmarSenha: '' };
    this.passwordModalOpen.set(true);
    this.userMenuOpen.set(false);
  }

  salvarPerfil() {
    this.http.put<UsuarioInfo>(`${this.api}/auth/me`, this.profileForm).subscribe({
      next: (usuario) => {
        this.usuario.set(usuario);
        sessionStorage.setItem('uniflow.usuario', JSON.stringify(usuario));
        this.profileModalOpen.set(false);
        this.notificar('Perfil atualizado com sucesso.', 'success');
      },
      error: () => this.notificar('Não foi possível atualizar o perfil.', 'error'),
    });
  }

  alterarSenha() {
    if (this.passwordForm.novaSenha !== this.passwordForm.confirmarSenha) {
      this.notificar('A confirmação da senha não confere.', 'error');
      return;
    }

    this.http
      .post(`${this.api}/auth/change-password`, {
        senhaAtual: this.passwordForm.senhaAtual,
        novaSenha: this.passwordForm.novaSenha,
      })
      .subscribe({
        next: () => {
          this.passwordModalOpen.set(false);
          this.notificar('Senha alterada com sucesso.', 'success');
        },
        error: () => this.notificar('Não foi possível alterar a senha.', 'error'),
      });
  }

  carregarDados() {
    this.mensagem.set('');
    this.setLoading(true);

    if (this.isSistemaAdmin()) {
      this.empresaLogada.set(null);
      sessionStorage.removeItem('uniflow.empresa');
      this.http.get<any>(`${this.api}/contratos/dashboard`).subscribe((x) => this.metricas.set(x));
      this.http.get<Empresa[]>(`${this.api}/empresas`).subscribe((x) => this.empresas.set(x ?? []));
      this.http.get<Plano[]>(`${this.api}/contratos/planos`).subscribe((x) => this.planos.set(x ?? []));
      this.http.get<Contrato[]>(`${this.api}/contratos`).subscribe({
        next: (x) => this.contratos.set(x ?? []),
        error: () => this.notificar('Não foi possível carregar contratos.', 'error'),
      });
      this.http.get<Cobranca[]>(`${this.api}/contratos/cobrancas`).subscribe((x) => this.cobrancas.set(x ?? []));
      this.carregarUsuarios();
      this.setLoading(false);
      return;
    }

    this.carregarEmpresaLogada();
    this.http.get<Contrato | null>(`${this.api}/contratos/minha`).subscribe((x) => this.contratoAtivoEmpresa.set(x ?? null));
    this.http.get<Cobranca[]>(`${this.api}/contratos/cobrancas/minhas`).subscribe((x) => this.cobrancasEmpresa.set(x ?? []));
    this.http.get<any[]>(`${this.api}/filiais`).subscribe((x) => this.filiais.set(x ?? []));
    this.http.get<any[]>(`${this.api}/departamentos`).subscribe((x) => this.departamentos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/cargos`).subscribe((x) => this.cargos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/colaboradores`).subscribe((x) => this.colaboradores.set(x ?? []));
    this.http.get<any[]>(`${this.api}/beneficios`).subscribe((x) => this.beneficios.set(x ?? []));
    this.http.get<any[]>(`${this.api}/vagas`).subscribe((x) => this.vagas.set(x ?? []));
    this.http.get<any[]>(`${this.api}/candidatos`).subscribe((x) => this.candidatos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/curriculos`).subscribe((x) => this.curriculos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/treinamentos`).subscribe((x) => this.treinamentos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/treinamentosColaboradores`).subscribe((x) => this.treinamentosColaboradores.set(x ?? []));
    this.http.get<any[]>(`${this.api}/epis`).subscribe((x) => this.epis.set(x ?? []));
    this.http.get<any[]>(`${this.api}/cargosEpis`).subscribe((x) => this.cargosEpis.set(x ?? []));
    this.http.get<any[]>(`${this.api}/colaboradoresEpis`).subscribe((x) => this.colaboradoresEpis.set(x ?? []));
    this.http.get<any[]>(`${this.api}/ferramentasAcesso`).subscribe((x) => this.ferramentasAcesso.set(x ?? []));
    this.http
      .get<any[]>(`${this.api}/colaboradoresFerramentasAcesso`)
      .subscribe((x) => this.colaboradoresFerramentasAcesso.set(x ?? []));
    this.http.get<any[]>(`${this.api}/etapasProcessosConfig`).subscribe((x) => this.etapasProcessosConfig.set(x ?? []));
    this.http.get<any[]>(`${this.api}/admissoes`).subscribe((x) => {
      const admissoes = x ?? [];
      this.admissoes.set(admissoes);
      this.sincronizarDocumentosAnexadosAdmissao(admissoes);
    });
    this.http.get<any[]>(`${this.api}/demissoes`).subscribe((x) => this.demissoes.set(x ?? []));
    this.http.get<any[]>(`${this.api}/documentosInstitucionais`).subscribe((x) => this.documentosInstitucionais.set(x ?? []));
    this.http.get<any[]>(`${this.api}/ferias`).subscribe((x) => this.ferias.set(x ?? []));
    this.http.get<any[]>(`${this.api}/registrosPonto`).subscribe((x) => this.pontos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/documentosColaboradores`).subscribe((x) => this.documentos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/solicitacoes`).subscribe((x) => this.solicitacoes.set(x ?? []));
    this.http.get<any[]>(`${this.api}/beneficiosColaboradores`).subscribe({
      next: (x) => {
        this.beneficiosColaboradores.set(x ?? []);
        this.setLoading(false);
      },
      error: () => this.notificar('Não foi possível carregar dados da empresa.', 'error'),
    });
    this.carregarUsuarios();
  }

  carregarUsuarios() {
    this.http.get<UsuarioInfo[]>(`${this.api}/usuarios`).subscribe({
      next: (x) => this.usuarios.set(x ?? []),
      error: () => this.usuarios.set([]),
    });
  }

  carregarEmpresaLogada() {
    this.http.get<Empresa>(`${this.api}/empresas/minha`).subscribe({
      next: (empresa) => {
        this.empresaLogada.set(empresa);
        sessionStorage.setItem('uniflow.empresa', JSON.stringify(empresa));
      },
      error: () => {
        this.empresaLogada.set(null);
        sessionStorage.removeItem('uniflow.empresa');
      },
    });
  }

  selecionarFotoPerfil(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = () => {
      const result = String(reader.result ?? '');
      this.profilePhotoUrl.set(result);
      localStorage.setItem('uniflow.profilePhoto', result);
      this.notificar('Foto atualizada na barra superior.', 'success');
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  selecionarFotoColaborador(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.notificar('Selecione um arquivo de imagem.', 'error');
      input.value = '';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      this.colaboradorForm.fotoUrl = String(reader.result ?? '');
      this.notificar('Foto do colaborador carregada.', 'success');
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  salvarEmpresa() {
    this.salvarCrud<Empresa>('empresas', this.empresaForm, () => (this.empresaForm = this.novaEmpresa()));
    this.empresaModalOpen.set(false);
  }

  abrirNovaEmpresaModal() {
    this.empresaForm = this.novaEmpresa();
    this.empresaModalOpen.set(true);
  }

  selecionarEmpresa(item: Empresa) {
    this.empresaForm = { ...item };
    this.empresasTab.set('dados');
    this.scrollTop();
  }

  editarEmpresa(item: Empresa) {
    this.empresaForm = { ...item };
    this.empresaModalOpen.set(true);
  }

  salvarPlano() {
    this.salvarCrud<Plano>('contratos/planos', this.planoForm, () => (this.planoForm = this.novoPlano()));
    this.planoModalOpen.set(false);
  }

  abrirNovoPlanoModal() {
    this.planoForm = this.novoPlano();
    this.planoModalOpen.set(true);
  }

  selecionarPlano(item: Plano) {
    this.planoForm = { ...item };
    this.activeModule.set('planos');
    this.planosTab.set('dados');
    this.scrollTop();
  }

  editarPlano(item: Plano) {
    this.planoForm = { ...item };
    this.planoModalOpen.set(true);
  }

  aplicarPlanoContrato(planoId?: number | null) {
    this.contratoForm.planoId = planoId ?? null;
    const plano = this.planos().find((x) => x.id === this.contratoForm.planoId);
    if (!plano) return;

    this.contratoForm.plano = plano.nome;
    this.contratoForm.limiteColaboradores = plano.limiteColaboradores;
    this.contratoForm.valorMensal = plano.valorCobranca;
    if (this.contratoForm.dataInicio) {
      const fim = new Date(this.contratoForm.dataInicio);
      fim.setDate(fim.getDate() + plano.prazoDias);
      this.contratoForm.dataFim = fim.toISOString().slice(0, 10);
    }
  }

  salvarContrato() {
    this.salvarCrud<Contrato>('contratos', this.contratoForm, () => (this.contratoForm = this.novoContrato()));
    this.contratoModalOpen.set(false);
  }

  abrirNovoContratoModal() {
    this.contratoForm = this.novoContrato();
    this.contratoModalOpen.set(true);
  }

  selecionarContrato(item: Contrato) {
    this.contratoForm = { ...item, dataFim: this.toDateInput(item.dataFim), dataInicio: this.toDateInput(item.dataInicio) };
    this.activeModule.set('contratacoes');
    this.contratacoesTab.set('dados');
    this.scrollTop();
  }

  editarContrato(item: Contrato) {
    this.contratoForm = { ...item, dataFim: this.toDateInput(item.dataFim), dataInicio: this.toDateInput(item.dataInicio) };
    this.contratoModalOpen.set(true);
  }

  salvarCobranca() {
    this.normalizeDateTime(this.cobrancaForm, ['dataGeracao', 'dataVencimento']);
    this.salvarCrud<Cobranca>('contratos/cobrancas', this.cobrancaForm, () => (this.cobrancaForm = this.novaCobranca()));
    this.cobrancaModalOpen.set(false);
  }

  abrirNovaCobrancaModal() {
    this.cobrancaForm = this.novaCobranca();
    this.cobrancaModalOpen.set(true);
  }

  selecionarCobranca(item: Cobranca) {
    this.cobrancaForm = {
      ...item,
      dataGeracao: this.toDateInput(item.dataGeracao),
      dataVencimento: this.toDateInput(item.dataVencimento),
    };
    this.activeModule.set('cobrancas');
    this.cobrancasTab.set('dados');
    this.scrollTop();
  }

  editarCobranca(item: Cobranca) {
    this.cobrancaForm = {
      ...item,
      dataGeracao: this.toDateInput(item.dataGeracao),
      dataVencimento: this.toDateInput(item.dataVencimento),
    };
    this.cobrancaModalOpen.set(true);
  }

  gerarContrato(contrato?: Contrato) {
    const selecionado = contrato ?? (this.contratoForm.id ? this.contratoForm : this.filteredContratos()[0]);
    if (!selecionado?.id) {
      this.notificar('Selecione um contrato para gerar o documento.', 'error');
      return;
    }

    this.contratoGerado.set(selecionado);
    this.contratoPreviewModalOpen.set(true);
  }

  imprimirContratoGerado() {
    const contrato = this.contratoGerado();
    if (!contrato) return;
    this.imprimirHtml(this.htmlContratoComercial(contrato));
  }

  baixarContratoPdf() {
    const contrato = this.contratoGerado();
    if (!contrato) return;
    this.notificar('Use a opção "Salvar como PDF" na janela de impressão.', 'info');
    this.imprimirContratoGerado();
  }

  enviarContratoEmail() {
    const contrato = this.contratoGerado();
    if (!contrato) return;
    const empresa = contrato.empresa ?? this.empresas().find((x) => x.id === contrato.empresaId);
    const assunto = encodeURIComponent(`Contrato UniFlow People - ${empresa?.nomeFantasia || contrato.plano}`);
    const corpo = encodeURIComponent(
      `Olá,\n\nSegue contrato comercial do plano ${contrato.plano} para assinatura via ${this.contratoAssinatura()}.\n\nValor mensal: ${this.formatCurrencyPrint(contrato.valorMensal)}\nVigência: ${this.formatDatePrint(contrato.dataInicio)} a ${this.formatDatePrint(contrato.dataFim)}\n\nAtenciosamente,\nUniFlow People`,
    );
    window.location.href = `mailto:${empresa?.email ?? ''}?subject=${assunto}&body=${corpo}`;
  }

  definirAssinaturaContrato(value: string) {
    this.contratoAssinatura.set(value === 'Certificado digital' ? 'Certificado digital' : 'GOV.BR');
  }

  formatCurrencyPrint(value: any) {
    return Number(value ?? 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }

  private htmlContratoComercial(contrato: Contrato) {
    const empresa = contrato.empresa ?? this.empresas().find((x) => x.id === contrato.empresaId);
    const assinatura = this.contratoAssinatura();
    const contratante = this.escapeHtml(empresa?.razaoSocial || empresa?.nomeFantasia || 'Empresa contratante');
    const cnpj = this.escapeHtml(empresa?.cnpj || 'CNPJ não informado');
    const endereco = this.escapeHtml([empresa?.endereco, empresa?.cidade, empresa?.estado, empresa?.cep].filter(Boolean).join(', ') || 'Endereço não informado');

    return `
      <main class="document-template-page">
        <h1>Contrato de Prestação de Serviços SaaS - UniFlow People</h1>
        <div class="document-template-body">
          <p><strong>Contratada:</strong> UniFlow People, plataforma de gestão de pessoas, doravante denominada CONTRATADA.</p>
          <p><strong>Contratante:</strong> ${contratante}, inscrita no CNPJ sob nº ${cnpj}, com endereço em ${endereco}, doravante denominada CONTRATANTE.</p>
          <p><strong>Objeto:</strong> disponibilização de acesso à plataforma UniFlow People para gestão de rotinas de RH, cadastros, documentos, processos, indicadores e módulos habilitados no plano contratado.</p>
          <p><strong>Plano contratado:</strong> ${this.escapeHtml(contrato.plano)}. Limite operacional de ${this.escapeHtml(contrato.limiteColaboradores)} pessoas cadastradas, observadas as regras de uso aceitável, segurança da informação e disponibilidade do serviço.</p>
          <p><strong>Vigência:</strong> de ${this.formatDatePrint(contrato.dataInicio)} até ${this.formatDatePrint(contrato.dataFim)}, renovável mediante concordância entre as partes ou continuidade de uso e pagamento.</p>
          <p><strong>Remuneração:</strong> valor mensal de ${this.formatCurrencyPrint(contrato.valorMensal)}, com cobranças emitidas conforme calendário financeiro aplicável e vencimentos informados à CONTRATANTE.</p>
          <p><strong>Responsabilidades da CONTRATADA:</strong> manter a plataforma disponível, aplicar boas práticas de segurança, prover manutenção evolutiva e corretiva, e proteger os dados tratados conforme legislação aplicável.</p>
          <p><strong>Responsabilidades da CONTRATANTE:</strong> manter dados cadastrais corretos, controlar acessos de seus usuários, utilizar a plataforma de forma lícita e quitar os valores contratados nos prazos acordados.</p>
          <p><strong>Proteção de dados:</strong> as partes comprometem-se a cumprir a LGPD, tratando dados pessoais somente para finalidades legítimas relacionadas à execução deste contrato.</p>
          <p><strong>Assinatura:</strong> este instrumento poderá ser assinado eletronicamente por ${this.escapeHtml(assinatura)}, com validade jurídica reconhecida pelas partes.</p>
          <p><strong>Observações comerciais:</strong> ${this.escapeHtml(contrato.observacoes || 'Sem observações adicionais.')}</p>
          <div class="signatures">
            <span>CONTRATANTE<br />${contratante}</span>
            <span>CONTRATADA<br />UniFlow People</span>
          </div>
        </div>
      </main>
    `;
  }

  salvarUsuario() {
    if (!this.isSistemaAdmin()) this.usuarioForm.empresaId = this.usuario()?.empresaId ?? undefined;

    const editando = !!this.usuarioForm.id;
    const payload = this.normalizePayload(this.usuarioForm);
    const request = editando
      ? this.http.put(`${this.api}/usuarios/${this.usuarioForm.id}`, payload)
      : this.http.post<UsuarioInfo>(`${this.api}/usuarios`, payload);

    request.subscribe({
      next: () => {
        this.usuarioForm = this.novoUsuario();
        this.usuarioModalOpen.set(false);
        this.carregarDados();
        this.notificar(editando ? 'Usuário atualizado com sucesso.' : 'Usuário criado com sucesso.', 'success');
      },
      error: () => this.notificar('Erro ao salvar usuário.', 'error'),
    });
  }

  editarUsuario(item: UsuarioInfo) {
    this.usuarioForm = {
      id: item.id,
      empresaId: item.empresaId ?? undefined,
      colaboradorId: item.colaboradorId ?? undefined,
      nome: item.nome,
      login: item.login,
      email: item.email,
      senha: '',
      role: item.role,
      ativo: true,
    };
    this.usuarioModalOpen.set(true);
  }

  abrirNovoUsuarioModal() {
    this.usuarioForm = this.novoUsuario();
    this.usuarioModalOpen.set(true);
  }

  salvarFilial() {
    this.salvarCrud('filiais', this.filialForm, () => (this.filialForm = this.novaFilial()));
  }

  salvarDepartamento() {
    this.salvarCrud('departamentos', this.departamentoForm, () => (this.departamentoForm = this.novoDepartamento()));
  }

  salvarCargo() {
    this.salvarCrud('cargos', this.cargoForm, () => (this.cargoForm = this.novoCargo()));
  }

  abrirNovoCargoModal() {
    this.cargoForm = this.novoCargo();
    this.genericEditModal.set({ form: 'cargoForm', title: 'Novo cargo' });
  }

  abrirNovoDepartamentoModal() {
    this.departamentoForm = this.novoDepartamento();
    this.genericEditModal.set({ form: 'departamentoForm', title: 'Novo departamento' });
  }

  salvarColaborador() {
    this.prepararStatusColaborador();
    if (this.cpfColaboradorJaExiste()) {
      this.marcarCpfDuplicado();
      return;
    }
    const editando = !!this.colaboradorForm.id;
    const payload = this.normalizePayload(this.colaboradorForm);
    const request = editando
      ? this.http.put(`${this.api}/colaboradores/${this.colaboradorForm.id}`, payload)
      : this.http.post(`${this.api}/colaboradores`, payload);

    request.subscribe({
      next: () => {
        const colaboradorAtualizado = { ...this.colaboradorForm };
        this.cpfColaboradorDuplicado.set(false);
        this.colaboradorForm = editando ? colaboradorAtualizado : this.novoColaborador();
        this.colaboradorModalOpen.set(false);
        this.colaboradoresTab.set(editando ? 'dados' : 'pesquisa');
        this.carregarDados();
        this.notificar(editando ? 'Colaborador atualizado com sucesso.' : 'Colaborador cadastrado com sucesso.', 'success');
      },
      error: (error) => this.tratarErroSalvarColaborador(error),
    });
  }

  abrirNovoColaboradorModal() {
    this.colaboradorForm = this.novoColaborador();
    this.colaboradoresTab.set('pesquisa');
    this.colaboradorModalOpen.set(true);
  }

  abrirModalColaboradorAdmissao(item?: any) {
    this.admissaoSelecionada = item ?? null;
    const vaga = this.vagaDaAdmissao(item);
    this.colaboradorForm = {
      ...this.novoColaborador(),
      nome: item?.nomeCandidato ?? '',
      telefone: item?.telefone ?? '',
      email: item?.email ?? '',
      departamentoId: vaga?.departamentoId,
      cargoId: vaga?.cargoId,
      dataAdmissao: this.toDateInput(item?.dataPrevistaAdmissao) || new Date().toISOString().slice(0, 10),
      observacoes: item?.cargoPretendido ? `Cargo pretendido: ${item.cargoPretendido}` : '',
    };
    this.admissaoColaboradorModalOpen.set(true);
  }

  salvarColaboradorAdmissao() {
    const admissao = this.admissaoSelecionada;
    this.prepararStatusColaborador();
    if (this.cpfColaboradorJaExiste()) {
      this.marcarCpfDuplicado();
      return;
    }
    this.http.post<any>(`${this.api}/colaboradores`, this.normalizePayload(this.colaboradorForm)).subscribe({
      next: (colaborador) => {
        this.cpfColaboradorDuplicado.set(false);
        this.colaboradorForm = this.novoColaborador();
        this.admissaoColaboradorModalOpen.set(false);
        this.concluirAdmissaoComColaborador(colaborador, admissao);
      },
      error: (error) => this.tratarErroSalvarColaborador(error, 'Erro ao cadastrar colaborador pela admissão.'),
    });
  }

  editarColaborador(item: any) {
    this.colaboradorForm = {
      ...item,
      dataNascimento: this.toDateInput(item.dataNascimento),
      dataAdmissao: this.toDateInput(item.dataAdmissao),
      dataDemissao: this.toDateInput(item.dataDemissao),
    };
    this.colaboradorModalOpen.set(true);
  }

  selecionarColaborador(item: any) {
    this.colaboradorForm = {
      ...item,
      dataNascimento: this.toDateInput(item.dataNascimento),
      dataAdmissao: this.toDateInput(item.dataAdmissao),
      dataDemissao: this.toDateInput(item.dataDemissao),
    };
    this.colaboradoresTab.set('dados');
  }

  salvarBeneficio() {
    this.salvarCrud('beneficios', this.beneficioForm, () => (this.beneficioForm = this.novoBeneficio()));
  }

  salvarVaga() {
    this.salvarCrud('vagas', this.vagaForm, () => {
      this.vagaForm = this.novaVaga();
      this.vagaModalOpen.set(false);
    });
  }

  editarVaga(item: any) {
    this.vagaForm = { ...item, dataEncerramento: this.toDateInput(item.dataEncerramento) };
    this.recrutamentoCadastro.set('vagas');
    this.vagaModalOpen.set(true);
  }

  novaVagaRecrutamento() {
    this.vagaForm = this.novaVaga();
    this.recrutamentoCadastro.set('vagas');
    this.vagaModalOpen.set(true);
  }

  editarCandidato(item: any) {
    this.candidatoForm = { ...item };
    this.recrutamentoCadastro.set('candidatos');
    this.candidatoModalOpen.set(true);
  }

  novoCandidatoRecrutamento() {
    this.candidatoForm = this.novoCandidato();
    this.recrutamentoCadastro.set('candidatos');
    this.candidatoModalOpen.set(true);
  }

  novoCurriculoRecrutamento() {
    this.curriculoForm = this.novoCurriculo();
    this.curriculoArquivo = null;
    this.recrutamentoCadastro.set('curriculos');
    this.curriculoModalOpen.set(true);
  }

  editarCurriculo(item: any) {
    this.curriculoForm = { ...item };
    this.curriculoArquivo = null;
    this.recrutamentoCadastro.set('curriculos');
    this.curriculoModalOpen.set(true);
  }

  salvarCandidato() {
    const editando = !!this.candidatoForm.id;
    const payload = this.normalizePayload(this.candidatoForm);
    const request = editando
      ? this.http.put(`${this.api}/candidatos/${this.candidatoForm.id}`, payload)
      : this.http.post<any>(`${this.api}/candidatos`, payload);

    request.subscribe({
      next: (candidato) => {
        const candidatoCriado = editando ? this.candidatoForm : candidato;
        this.candidatoForm = this.novoCandidato();
        this.candidatoModalOpen.set(false);
        if (editando) {
          this.carregarDados();
          this.recrutamentoTab.set('pesquisa');
          this.notificar('Candidato atualizado com sucesso.', 'success');
          return;
        }

        this.carregarDados();
        this.recrutamentoTab.set('pesquisa');
        this.notificar('Candidato cadastrado com sucesso.', 'success');
      },
      error: () => this.notificar('Erro ao salvar candidatos.', 'error'),
    });
  }

  selecionarCurriculoArquivo(event: Event) {
    const input = event.target as HTMLInputElement;
    this.curriculoArquivo = input.files?.[0] ?? null;
  }

  salvarCurriculo() {
    const editando = !!this.curriculoForm.id;
    const form = new FormData();
    form.append('nome', this.curriculoForm.nome ?? '');
    form.append('telefone', this.curriculoForm.telefone ?? '');
    form.append('email', this.curriculoForm.email ?? '');
    form.append('status', this.curriculoForm.status ?? 'Disponivel');
    if (this.curriculoArquivo) form.append('arquivo', this.curriculoArquivo);

    const request = editando
      ? this.http.put(`${this.api}/curriculos/${this.curriculoForm.id}`, form)
      : this.http.post(`${this.api}/curriculos`, form);

    request.subscribe({
      next: () => {
        this.curriculoForm = this.novoCurriculo();
        this.curriculoArquivo = null;
        this.curriculoModalOpen.set(false);
        this.carregarDados();
        this.notificar(editando ? 'Currículo atualizado com sucesso.' : 'Currículo cadastrado com sucesso.', 'success');
      },
      error: () => this.notificar('Erro ao salvar currículo.', 'error'),
    });
  }

  salvarTreinamento() {
    this.normalizeDateTime(this.treinamentoForm, ['dataInicio', 'dataFim']);
    this.salvarCrud('treinamentos', this.treinamentoForm, () => {
      this.treinamentoForm = this.novoTreinamento();
      this.treinamentoModalOpen.set(false);
    });
  }

  abrirNovoTreinamentoModal() {
    this.treinamentoForm = this.novoTreinamento();
    this.treinamentoModalOpen.set(true);
  }

  editarTreinamento(item: any) {
    this.treinamentoForm = {
      ...item,
      dataInicio: this.toDateTimeInput(item.dataInicio),
      dataFim: this.toDateTimeInput(item.dataFim),
    };
    this.treinamentoModalOpen.set(true);
  }

  vincularTreinamentoColaborador() {
    this.http.post(`${this.api}/treinamentosColaboradores`, this.normalizePayload(this.treinamentoColaboradorForm)).subscribe({
      next: () => {
        this.treinamentoColaboradorForm = this.novoTreinamentoColaborador();
        this.carregarDados();
        this.notificar('Colaborador vinculado ao treinamento.', 'success');
      },
      error: () => this.notificar('Erro ao vincular colaborador ao treinamento.', 'error'),
    });
  }

  marcarPresenca(item: any, presente: boolean) {
    this.http
      .put(`${this.api}/treinamentosColaboradores/${item.id}/presenca`, { ...item, presente, status: presente ? 'Presente' : 'Ausente' })
      .subscribe({
        next: () => {
          this.carregarDados();
          this.notificar('Presença atualizada.', 'success');
        },
        error: () => this.notificar('Erro ao atualizar presença.', 'error'),
      });
  }

  salvarEpi() {
    this.salvarCrud('epis', this.epiForm, () => {
      this.epiForm = this.novoEpi();
      this.epiModalOpen.set(false);
    });
  }

  abrirNovoEpiModal() {
    this.epiForm = this.novoEpi();
    this.epiModalOpen.set(true);
  }

  editarEpi(item: any) {
    this.epiForm = { ...item };
    this.epiModalOpen.set(true);
  }

  salvarCargoEpi() {
    this.salvarCrud('cargosEpis', this.cargoEpiForm, () => {
      this.cargoEpiForm = this.novoCargoEpi();
      this.cargoEpiModalOpen.set(false);
    });
  }

  abrirNovoCargoEpiModal(cargoId?: number) {
    this.cargoEpiForm = { ...this.novoCargoEpi(), cargoId };
    this.cargoEpiModalOpen.set(true);
  }

  editarCargoEpi(item: any) {
    this.cargoEpiForm = { ...item };
    this.cargoEpiModalOpen.set(true);
  }

  salvarColaboradorEpi() {
    const form = this.colaboradorEpiForm;
    this.normalizeDateTime(form, ['dataRetirada', 'dataPrevistaTroca', 'dataDevolucao']);
    this.salvarCrud('colaboradoresEpis', form, () => {
      this.colaboradorEpiForm = this.novoColaboradorEpi();
      this.colaboradorEpiModalOpen.set(false);
    });
  }

  abrirNovoColaboradorEpiModal(colaboradorId?: number, epiId?: number) {
    this.colaboradorEpiForm = { ...this.novoColaboradorEpi(), colaboradorId, epiId };
    this.colaboradorEpiModalOpen.set(true);
  }

  editarColaboradorEpi(item: any) {
    this.colaboradorEpiForm = {
      ...item,
      dataRetirada: this.toDateInput(item.dataRetirada),
      dataPrevistaTroca: this.toDateInput(item.dataPrevistaTroca),
      dataDevolucao: this.toDateInput(item.dataDevolucao),
    };
    this.colaboradorEpiModalOpen.set(true);
  }

  salvarFerramentaAcesso() {
    this.salvarCrud('ferramentasAcesso', this.ferramentaAcessoForm, () => {
      this.ferramentaAcessoForm = this.novaFerramentaAcesso();
      this.ferramentaAcessoModalOpen.set(false);
    });
  }

  abrirNovaFerramentaAcessoModal() {
    this.ferramentaAcessoForm = this.novaFerramentaAcesso();
    this.ferramentaAcessoModalOpen.set(true);
  }

  editarFerramentaAcesso(item: any) {
    this.ferramentaAcessoForm = { ...item };
    this.ferramentaAcessoModalOpen.set(true);
  }

  salvarColaboradorFerramentaAcesso() {
    const form = this.colaboradorFerramentaAcessoForm;
    this.normalizeDateTime(form, ['dataEntrega', 'dataDevolucao']);
    this.salvarCrud('colaboradoresFerramentasAcesso', form, () => {
      this.colaboradorFerramentaAcessoForm = this.novoColaboradorFerramentaAcesso();
      this.colaboradorFerramentaAcessoModalOpen.set(false);
    });
  }

  abrirNovoColaboradorFerramentaAcessoModal(colaboradorId?: number) {
    this.colaboradorFerramentaAcessoForm = { ...this.novoColaboradorFerramentaAcesso(), colaboradorId };
    this.colaboradorFerramentaAcessoModalOpen.set(true);
  }

  editarColaboradorFerramentaAcesso(item: any) {
    this.colaboradorFerramentaAcessoForm = {
      ...item,
      dataEntrega: this.toDateInput(item.dataEntrega),
      dataDevolucao: this.toDateInput(item.dataDevolucao),
    };
    this.colaboradorFerramentaAcessoModalOpen.set(true);
  }

  salvarEtapaProcesso() {
    this.salvarCrud('etapasProcessosConfig', this.etapaProcessoForm, () => {
      this.etapaProcessoForm = this.novaEtapaProcesso();
      this.etapaProcessoModalOpen.set(false);
    });
  }

  abrirNovaEtapaProcessoModal() {
    this.etapaProcessoForm = this.novaEtapaProcesso();
    this.etapaProcessoModalOpen.set(true);
  }

  editarEtapaProcesso(item: any) {
    this.etapaProcessoForm = { ...item };
    this.etapaProcessoModalOpen.set(true);
  }

  etapasProcessosOrdenadas() {
    return [...this.etapasProcessosConfig()].sort((a, b) =>
      String(a.tipoProcesso).localeCompare(String(b.tipoProcesso)) || Number(a.ordem ?? 0) - Number(b.ordem ?? 0),
    );
  }

  abrirNovoProcessoAdmissao() {
    this.admissaoSelecionada = null;
    this.admissaoForm = this.novaAdmissao();
    this.admissaoProcessoModalOpen.set(true);
  }

  abrirModalProcessoAdmissao(item: any) {
    this.admissaoSelecionada = item;
    const vaga = this.vagaDaAdmissao(item);
    const candidato = this.candidatos().find(
      (x) =>
        x.vagaId === vaga?.id &&
        (String(x.email ?? '').toLowerCase() === String(item.email ?? '').toLowerCase() || x.nome === item.nomeCandidato),
    );
    this.admissaoForm = {
      ...item,
      vagaId: vaga?.id,
      candidatoId: candidato?.id,
      dataPrevistaAdmissao: this.toDateInput(item.dataPrevistaAdmissao),
    };
    this.admissaoProcessoModalOpen.set(true);
  }

  fecharModalProcessoAdmissao() {
    this.admissaoProcessoModalOpen.set(false);
    this.admissaoSelecionada = null;
    this.admissaoForm = this.novaAdmissao();
  }

  salvarAdmissao() {
    if (!this.admissaoForm.id && !this.admissaoForm.candidatoId) {
      this.notificar('Selecione uma vaga aberta e um candidato para criar o processo admissional.', 'error');
      return;
    }

    this.normalizeDateTime(this.admissaoForm, ['dataPrevistaAdmissao']);
    const editando = !!this.admissaoForm.id;
    const { vagaId: _vagaId, candidatoId: _candidatoId, ...processo } = this.admissaoForm;
    const payload = this.normalizePayload(processo);
    const request = editando
      ? this.http.put(`${this.api}/admissoes/${this.admissaoForm.id}`, payload)
      : this.http.post<any>(`${this.api}/admissoes`, payload);

    request.subscribe({
      next: () => {
        this.fecharModalProcessoAdmissao();
        this.carregarDados();
        this.notificar(editando ? 'Processo atualizado com sucesso.' : 'Processo criado com sucesso.', 'success');
      },
      error: () => this.notificar('Erro ao salvar processo de admissao.', 'error'),
    });
  }

  admitirColaborador(item: any) {
    if (!this.podeAdmitir(item)) {
      this.notificar('Conclua todas as etapas antes de efetuar a admissao.', 'error');
      return;
    }

    this.abrirModalColaboradorAdmissao(item);
    this.notificar('Complete o cadastro para concluir a admissão.', 'info');
  }

  abrirModalDocumentacaoAdmissao(item: any) {
    this.admissaoSelecionada = item;
    this.admissaoDocumentosModalOpen.set(true);
  }

  fecharModalDocumentacaoAdmissao() {
    this.admissaoDocumentosModalOpen.set(false);
    this.admissaoSelecionada = null;
  }

  documentosDaAdmissao(item: any) {
    return this.documentosInstitucionais().filter((x) => x.admissaoProcessoId === item?.id);
  }

  urlModeloDocumentoAdmissao(modelo: any) {
    const apiOrigin = this.api.replace(/\/api\/?$/, '');
    return `${apiOrigin}/modelos/admissao/${encodeURIComponent(modelo.arquivo)}`;
  }

  urlTemplateDocumentoAdmissao(modelo: any) {
    const apiOrigin = this.api.replace(/\/api\/?$/, '');
    return `${apiOrigin}/modelos/admissao/templates/${encodeURIComponent(modelo.template)}`;
  }

  colaboradorDaAdmissao(item: any) {
    return this.colaboradores().find((x) => x.id === item?.colaboradorId) ?? null;
  }

  nomeDocumentoAdmissao(item: any) {
    return this.colaboradorDaAdmissao(item)?.nome ?? item?.nomeCandidato ?? '';
  }

  cpfDocumentoAdmissao(item: any) {
    return this.colaboradorDaAdmissao(item)?.cpf ?? item?.cpf ?? '';
  }

  imprimirModeloDocumentoAdmissao(modelo: any, item: any) {
    this.http.get(this.urlTemplateDocumentoAdmissao(modelo), { responseType: 'text' }).subscribe({
      next: (template) => this.imprimirHtml(this.renderizarModeloDocumentoAdmissao(template, item)),
      error: () => this.notificar('Não foi possível carregar o texto do modelo para impressão.', 'error'),
    });
  }

  private renderizarModeloDocumentoAdmissao(template: string, item: any) {
    const nome = this.nomeDocumentoAdmissao(item) || '________________';
    const cpf = this.cpfDocumentoAdmissao(item) || '________________';
    const data = this.formatDatePrint(new Date());
    const texto = template
      .replace(/\{\{NOME\}\}/gi, nome)
      .replace(/\{\{CPF\}\}/gi, cpf)
      .replace(/\{\{DATA\}\}/gi, data);

    return `
      <section class="document-page document-template-page">
        <div class="document-template-body">${this.textoParaHtmlDocumento(texto)}</div>
      </section>
    `;
  }

  private textoParaHtmlDocumento(texto: string) {
    return this.escapeHtml(texto)
      .replace(/\r\n/g, '\n')
      .replace(/\r/g, '\n')
      .split(/\n{2,}/)
      .map((paragraph) => `<p>${paragraph.replace(/\n/g, '<br />')}</p>`)
      .join('');
  }

  gerarDocumentosAdmissao() {
    const item = this.admissaoSelecionada;
    if (!item?.id) return;

    this.http.post(`${this.api}/admissoes/${item.id}/documentos`, {}).subscribe({
      next: () => {
        this.carregarDados();
        this.notificar('Documentacao institucional gerada com sucesso.', 'success');
      },
      error: () => this.notificar('Erro ao gerar documentacao institucional.', 'error'),
    });
  }

  abrirNovoProcessoDemissao() {
    this.demissaoForm = this.novaDemissao();
    this.demissaoProcessoModalOpen.set(true);
  }

  fecharModalProcessoDemissao() {
    this.demissaoProcessoModalOpen.set(false);
    this.demissaoForm = this.novaDemissao();
  }

  salvarDemissao() {
    this.normalizeDateTime(this.demissaoForm, ['dataSolicitacao', 'dataDesligamento']);
    this.salvarCrud('demissoes', this.demissaoForm, () => {
      this.fecharModalProcessoDemissao();
      this.demissaoPage.set(1);
    });
  }

  editarDemissao(item: any) {
    this.demissaoForm = {
      ...item,
      dataSolicitacao: this.toDateInput(item.dataSolicitacao),
      dataDesligamento: this.toDateInput(item.dataDesligamento),
    };
    this.demissaoProcessoModalOpen.set(true);
  }

  concluirDemissao(item: any) {
    if (!this.podeConcluirDemissao(item)) {
      this.notificar('Conclua todas as etapas antes de efetivar a demissao.', 'error');
      return;
    }

    this.http.post(`${this.api}/demissoes/${item.id}/concluir`, {}).subscribe({
      next: () => {
        this.demissaoEtapaSelecionada.set(null);
        this.carregarDados();
        this.notificar('Fluxo demissional concluído e colaborador inativado.', 'success');
      },
      error: () => this.notificar('Erro ao concluir demissão.', 'error'),
    });
  }

  cancelarProcessoDemissao(item: any) {
    if (!item?.id || this.processoDemissaoFinalizado(item)) return;

    this.http.post(`${this.api}/demissoes/${item.id}/cancelar`, {}).subscribe({
      next: () => {
        item.status = 'Cancelado';
        this.demissaoEtapaSelecionada.set(null);
        this.carregarDados();
        this.notificar('Processo demissional cancelado.', 'success');
      },
      error: () => this.notificar('Erro ao cancelar processo demissional.', 'error'),
    });
  }

  salvarFerias() {
    this.salvarCrud('ferias', this.feriasForm, () => (this.feriasForm = this.novaFerias()));
  }

  salvarPonto() {
    this.pontoForm.dataHora = new Date(this.pontoForm.dataHora).toISOString();
    this.http.post(`${this.api}/registrosPonto`, this.pontoForm).subscribe({
      next: () => {
        this.pontoForm = this.novoPonto();
        this.carregarDados();
        this.notificar('Registro de ponto lançado.', 'success');
      },
      error: () => this.notificar('Erro ao lançar ponto.', 'error'),
    });
  }

  salvarDocumento() {
    const editando = !!this.documentoForm.id;
    const payload = this.normalizePayload(this.documentoForm);
    const request = editando
      ? this.http.put(`${this.api}/documentosColaboradores/${this.documentoForm.id}`, payload)
      : this.http.post(`${this.api}/documentosColaboradores`, payload);

    request.subscribe({
      next: () => {
        this.documentoForm = this.novoDocumento();
        this.documentoModalOpen.set(false);
        this.carregarDados();
        this.notificar(editando ? 'Documento atualizado.' : 'Documento salvo.', 'success');
      },
      error: () => this.notificar('Erro ao salvar documento.', 'error'),
    });
  }

  abrirNovoDocumentoModal() {
    this.documentoForm = this.novoDocumento();
    this.documentoModalOpen.set(true);
  }

  editarDocumento(item: any) {
    this.documentoForm = { ...item };
    this.documentoModalOpen.set(true);
  }

  salvarSolicitacao() {
    if (!this.podeGerenciarSolicitacoes()) {
      this.solicitacaoForm.status = 'Enviada';
      this.solicitacaoForm.respostaRh = null;
      this.solicitacaoForm.colaboradorId = this.usuario()?.colaboradorId ?? this.solicitacaoForm.colaboradorId;
    }

    const editando = !!this.solicitacaoForm.id;
    const payload = this.normalizePayload(this.solicitacaoForm);
    const request = editando
      ? this.http.put(`${this.api}/solicitacoes/${this.solicitacaoForm.id}`, payload)
      : this.http.post(`${this.api}/solicitacoes`, payload);

    request.subscribe({
      next: () => {
        this.solicitacaoForm = this.novaSolicitacao();
        this.solicitacaoModalOpen.set(false);
        this.carregarDados();
        this.notificar(editando ? 'Solicitação atualizada.' : 'Solicitação enviada ao RH.', 'success');
      },
      error: () => this.notificar('Erro ao salvar solicitação.', 'error'),
    });
  }

  editarSolicitacao(item: any) {
    this.solicitacaoForm = { ...item };
    this.solicitacaoModalOpen.set(true);
  }

  abrirNovaSolicitacaoModal() {
    this.solicitacaoForm = this.novaSolicitacao();
    this.solicitacaoModalOpen.set(true);
  }

  limparSolicitacao() {
    this.solicitacaoForm = this.novaSolicitacao();
  }

  podeGerenciarSolicitacoes() {
    const role = this.usuario()?.role;
    return role === 'EmpresaAdmin' || role === 'RH';
  }

  salvarBeneficioColaborador() {
    const editando = !!this.beneficioColaboradorForm.id;
    const payload = this.normalizePayload(this.beneficioColaboradorForm);
    const request = editando
      ? this.http.put(`${this.api}/beneficiosColaboradores/${this.beneficioColaboradorForm.id}`, payload)
      : this.http.post(`${this.api}/beneficiosColaboradores`, payload);

    request.subscribe({
      next: () => {
        this.beneficioColaboradorForm = this.novoBeneficioColaborador();
        this.carregarDados();
        this.notificar(editando ? 'Vínculo atualizado.' : 'Benefício vinculado ao colaborador.', 'success');
      },
      error: () => this.notificar('Erro ao vincular benefício.', 'error'),
    });
  }

  editarGenerico(form: string, item: any) {
    (this as any)[form] = { ...item };
    if (form === 'beneficioColaboradorForm') {
      this.beneficioColaboradorForm.dataInicio = this.toDateInput(item.dataInicio);
      this.beneficioColaboradorForm.dataFim = this.toDateInput(item.dataFim);
    }
    if (form === 'feriasForm') {
      this.feriasForm.dataInicio = this.toDateInput(item.dataInicio);
      this.feriasForm.dataFim = this.toDateInput(item.dataFim);
      this.feriasForm.periodoAquisitivoInicio = this.toDateInput(item.periodoAquisitivoInicio);
      this.feriasForm.periodoAquisitivoFim = this.toDateInput(item.periodoAquisitivoFim);
    }
    this.genericEditModal.set({ form, title: this.tituloModalGenerico(form) });
  }

  fecharModalGenerico() {
    this.genericEditModal.set(null);
  }

  salvarModalGenerico(form: string) {
    const actions: Record<string, () => void> = {
      cargoForm: () => this.salvarCargo(),
      departamentoForm: () => this.salvarDepartamento(),
      filialForm: () => this.salvarFilial(),
      beneficioForm: () => this.salvarBeneficio(),
      beneficioColaboradorForm: () => this.salvarBeneficioColaborador(),
      feriasForm: () => this.salvarFerias(),
    };
    actions[form]?.();
    this.genericEditModal.set(null);
  }

  private tituloModalGenerico(form: string) {
    const titles: Record<string, string> = {
      cargoForm: 'Editar cargo',
      departamentoForm: 'Editar departamento',
      filialForm: 'Editar filial',
      beneficioForm: 'Editar benefício',
      beneficioColaboradorForm: 'Editar vínculo de benefício',
      feriasForm: 'Editar férias',
    };
    return titles[form] ?? 'Editar registro';
  }

  pedirExclusao(endpoint: string, id: number | undefined, label: string) {
    if (!id) return;
    this.confirmDelete.set({ endpoint, id, label });
  }

  confirmarExclusao() {
    const item = this.confirmDelete();
    if (!item) return;

    this.http.delete(`${this.api}/${item.endpoint}/${item.id}`).subscribe({
      next: () => {
        this.confirmDelete.set(null);
        this.carregarDados();
        this.notificar('Registro excluído.', 'success');
      },
      error: () => this.notificar('Não foi possível excluir o registro.', 'error'),
    });
  }

  nomeEmpresa(id?: number) {
    return this.empresas().find((x) => x.id === id)?.nomeFantasia ?? id ?? '-';
  }

  ultimaCobrancaEmpresa() {
    return this.cobrancasEmpresa()[0] ?? null;
  }

  clientesEmDia() {
    const atrasadas = new Set(this.cobrancasAtrasadas().map((x) => x.empresaId));
    return new Set(this.contratos().filter((x) => x.status === 'Ativo' && !atrasadas.has(x.empresaId)).map((x) => x.empresaId)).size;
  }

  clientesEmAtraso() {
    return new Set(this.cobrancasAtrasadas().map((x) => x.empresaId)).size;
  }

  cobrancasEmAberto() {
    return this.cobrancasAbertasLista().length;
  }

  cobrancasAbertasLista() {
    return this.cobrancas().filter((x) => x.status === 'Pendente');
  }

  contratosAVencerLista() {
    const hoje = new Date();
    const limite = new Date();
    limite.setDate(limite.getDate() + 30);
    return this.contratos().filter((x) => {
      if (!x.dataFim || x.status !== 'Ativo') return false;
      const vencimento = new Date(x.dataFim);
      return vencimento >= hoje && vencimento <= limite;
    });
  }

  private cobrancasAtrasadas() {
    const hoje = new Date();
    return this.cobrancas().filter((x) => x.status === 'Pendente' && new Date(x.dataVencimento) < hoje);
  }

  planosMaisUsados() {
    const total = Math.max(1, this.contratos().length);
    const porPlano = this.contratos().reduce<Record<string, number>>((acc, contrato) => {
      const plano = contrato.plano || 'Sem plano';
      acc[plano] = (acc[plano] ?? 0) + 1;
      return acc;
    }, {});

    return Object.entries(porPlano)
      .map(([nome, quantidade]) => ({ nome, quantidade, percentual: Math.round((quantidade / total) * 100) }))
      .sort((a, b) => b.quantidade - a.quantidade)
      .slice(0, 5);
  }

  percentualClientesEmDia() {
    const total = Math.max(1, this.clientesEmDia() + this.clientesEmAtraso());
    return Math.round((this.clientesEmDia() / total) * 100);
  }

  percentualClientesEmAtraso() {
    const total = Math.max(1, this.clientesEmDia() + this.clientesEmAtraso());
    return Math.round((this.clientesEmAtraso() / total) * 100);
  }

  movimentacaoPessoasDashboard() {
    const meses = this.ultimosMeses(6);
    return meses.map((mes) => {
      const admissoes = this.admissoes().filter((item) => this.mesDaData(item.dataPrevistaAdmissao) === mes.key).length;
      const demissoes = this.demissoes().filter((item) => this.mesDaData(item.dataDesligamento || item.dataSolicitacao) === mes.key).length;
      const maior = Math.max(1, admissoes, demissoes);

      return {
        ...mes,
        admissoes,
        demissoes,
        admissoesPercentual: Math.max(8, Math.round((admissoes / maior) * 100)),
        demissoesPercentual: Math.max(8, Math.round((demissoes / maior) * 100)),
      };
    });
  }

  beneficiosMaisUsadosEmpresa() {
    const total = Math.max(1, this.beneficiosColaboradores().length);
    const porBeneficio = this.beneficiosColaboradores().reduce<Record<string, { nome: string; quantidade: number }>>((acc, item) => {
      const key = String(item.beneficioId ?? 'sem-beneficio');
      const nome = this.nomeBeneficio(item.beneficioId);
      acc[key] = acc[key] ?? { nome: String(nome), quantidade: 0 };
      acc[key].quantidade += 1;
      return acc;
    }, {});

    return Object.values(porBeneficio)
      .map((item) => ({ ...item, percentual: Math.round((item.quantidade / total) * 100) }))
      .sort((a, b) => b.quantidade - a.quantidade)
      .slice(0, 5);
  }

  crescimentoColaboradoresDashboard() {
    const meses = this.ultimosMeses(6);
    const totais = meses.map((mes) => {
      const fimDoMes = new Date(mes.ano, mes.mes + 1, 0, 23, 59, 59);
      const quantidade = this.colaboradores().filter((colaborador) => {
        const admissao = colaborador.dataAdmissao ? new Date(colaborador.dataAdmissao) : null;
        const demissao = colaborador.dataDemissao ? new Date(colaborador.dataDemissao) : null;
        return !!admissao && admissao <= fimDoMes && (!demissao || demissao > fimDoMes);
      }).length;

      return { ...mes, quantidade };
    });
    const maior = Math.max(1, ...totais.map((item) => item.quantidade));
    return totais.map((item) => ({ ...item, percentual: Math.max(10, Math.round((item.quantidade / maior) * 100)) }));
  }

  feriasProximasDashboard() {
    const hoje = new Date();
    const limite = new Date();
    limite.setDate(limite.getDate() + 30);

    return this.ferias()
      .filter((item) => {
        if (!item.dataInicio || item.status === 'Cancelada') return false;
        const inicio = new Date(item.dataInicio);
        return inicio >= hoje && inicio <= limite;
      })
      .sort((a, b) => new Date(a.dataInicio).getTime() - new Date(b.dataInicio).getTime())
      .slice(0, 6);
  }

  feriasAVencerDashboard() {
    const hoje = new Date();
    const limite = new Date();
    limite.setDate(limite.getDate() + 60);

    return this.ferias()
      .filter((item) => {
        if (!item.periodoAquisitivoFim || item.status === 'Concluída' || item.status === 'Cancelada') return false;
        const vencimento = new Date(item.periodoAquisitivoFim);
        return vencimento >= hoje && vencimento <= limite;
      })
      .sort((a, b) => new Date(a.periodoAquisitivoFim).getTime() - new Date(b.periodoAquisitivoFim).getTime())
      .slice(0, 6);
  }

  nomeColaborador(id?: number) {
    return this.colaboradores().find((x) => x.id === id)?.nome ?? id ?? '-';
  }

  nomeCargo(id?: number) {
    return this.cargos().find((x) => x.id === id)?.nome ?? id ?? '-';
  }

  cargoExtinto(cargo: any) {
    return cargo?.ativo === false;
  }

  cargosSelecionaveis(cargoId?: number | null) {
    return this.cargos().filter((cargo) => cargo.ativo !== false || cargo.id === cargoId);
  }

  labelCargo(cargo: any) {
    return cargo?.ativo === false ? `${cargo.nome} (Extinto)` : cargo?.nome;
  }

  nomeEpi(id?: number) {
    return this.epis().find((x) => x.id === id)?.nome ?? id ?? '-';
  }

  nomeFerramentaAcesso(id?: number) {
    return this.ferramentasAcesso().find((x) => x.id === id)?.nome ?? id ?? '-';
  }

  episDoCargo(cargoId?: number) {
    if (!cargoId) return [];
    return this.cargosEpis().filter((x) => x.cargoId === cargoId);
  }

  episDoColaborador(colaboradorId?: number) {
    if (!colaboradorId) return [];
    return this.colaboradoresEpis().filter((x) => x.colaboradorId === colaboradorId);
  }

  episPendentesTroca(colaboradorId?: number) {
    const hoje = new Date();
    return this.episDoColaborador(colaboradorId).filter((x) => {
      if (x.status !== 'Retirado' || !x.dataPrevistaTroca) return false;
      return new Date(x.dataPrevistaTroca) <= hoje;
    });
  }

  ferramentasDoColaborador(colaboradorId?: number) {
    if (!colaboradorId) return [];
    return this.colaboradoresFerramentasAcesso().filter((x) => x.colaboradorId === colaboradorId);
  }

  aplicarEpisDoCargoNoColaborador() {
    const colaboradorId = this.colaboradorForm.id;
    const cargoId = this.colaboradorForm.cargoId;
    if (!colaboradorId || !cargoId) {
      this.notificar('Salve o colaborador e selecione um cargo antes de lançar EPIs do cargo.', 'error');
      return;
    }

    const jaRetirados = new Set(
      this.episDoColaborador(colaboradorId)
        .filter((x) => x.status === 'Retirado')
        .map((x) => x.epiId),
    );
    const pendentes = this.episDoCargo(cargoId).filter((x) => !jaRetirados.has(x.epiId));
    if (!pendentes.length) {
      this.notificar('Todos os EPIs obrigatórios do cargo já constam na ficha.', 'info');
      return;
    }

    forkJoin(
      pendentes.map((item) =>
        this.http.post(`${this.api}/colaboradoresEpis`, {
          colaboradorId,
          epiId: item.epiId,
          quantidade: item.quantidadePadrao || 1,
          dataRetirada: new Date().toISOString(),
          status: 'Retirado',
          observacoes: 'Lançado a partir do cargo do colaborador.',
        }),
      ),
    ).subscribe({
      next: () => {
        this.carregarDados();
        this.notificar('EPIs do cargo lançados na ficha do colaborador.', 'success');
      },
      error: () => this.notificar('Erro ao lançar EPIs do cargo.', 'error'),
    });
  }

  nomeDepartamento(id?: number) {
    return this.departamentos().find((x) => x.id === id)?.nome ?? id ?? '-';
  }

  nomeFilial(id?: number) {
    return this.filiais().find((x) => x.id === id)?.nome ?? id ?? '-';
  }

  nomeVaga(id?: number) {
    return this.vagas().find((x) => x.id === id)?.titulo ?? id ?? '-';
  }

  vagasAbertas() {
    return this.vagas().filter((x) => x.status === 'Aberta');
  }

  candidatosDaVaga(vagaId?: number) {
    return this.candidatos().filter((x) => x.vagaId === vagaId);
  }

  candidatosDaVagaAdmissao() {
    return this.candidatosDaVaga(this.admissaoForm.vagaId);
  }

  selecionarVagaAdmissao(vagaId: number | undefined) {
    const vaga = this.vagas().find((x) => x.id === vagaId);
    this.admissaoForm = {
      ...this.admissaoForm,
      vagaId,
      candidatoId: undefined,
      nomeCandidato: '',
      email: '',
      telefone: '',
      cargoPretendido: vaga?.titulo ?? '',
    };
  }

  selecionarCandidatoAdmissao(candidatoId: number | undefined) {
    const candidato = this.candidatos().find((x) => x.id === candidatoId);
    const vaga = this.vagas().find((x) => x.id === (candidato?.vagaId ?? this.admissaoForm.vagaId));
    this.admissaoForm = {
      ...this.admissaoForm,
      candidatoId,
      vagaId: candidato?.vagaId ?? this.admissaoForm.vagaId,
      nomeCandidato: candidato?.nome ?? '',
      email: candidato?.email ?? '',
      telefone: candidato?.telefone ?? '',
      cargoPretendido: vaga?.titulo ?? '',
    };
  }

  admissaoDoCandidato(candidato: any) {
    return this.admissoes().find(
      (x) =>
        String(x.email ?? '').toLowerCase() === String(candidato.email ?? '').toLowerCase() ||
        (x.nomeCandidato === candidato.nome && x.cargoPretendido === this.nomeVaga(candidato.vagaId)),
    );
  }

  etapasAdmissao(item: any) {
    const configs = this.etapasProcessosConfig()
      .filter((x) => x.tipoProcesso === 'Admissao' && x.ativa)
      .sort((a, b) => Number(a.ordem ?? 0) - Number(b.ordem ?? 0));
    const nomes = configs.length
      ? configs.map((x) => x.nome)
      : ['Candidato cadastrado', 'Entrevista com RH', 'Entrevista com gestor', 'Avaliacao psicologica', 'Anexar documentos'];
    const etapas = Array.isArray(item?.etapas) ? item.etapas : [];

    return nomes.map((nome, index) => {
      const etapa = etapas.find((x: any) => x.nome === nome);
      const config = configs.find((x) => x.nome === nome);
      return etapa ?? { id: 0, nome, status: config?.primeiraEtapaConcluida || index === 0 ? 'Concluida' : 'Bloqueada' };
    });
  }

  etapaLiberada(item: any, etapa: any) {
    if (this.processoFinalizado(item)) return false;
    const etapas = this.etapasAdmissao(item);
    const index = etapas.findIndex((x: any) => x.nome === etapa.nome);
    if (index <= 0) return true;
    return etapas.slice(0, index).every((x: any) => x.status === 'Concluida');
  }

  podeAdmitir(item: any) {
    return !this.processoFinalizado(item) && this.etapasAdmissao(item).every((x: any) => x.status === 'Concluida');
  }

  statusAdmissao(item: any) {
    if (item?.status === 'Admitido') return 'Admitido';
    if (item?.status === 'Cancelado') return 'Cancelado';
    return this.proximaEtapaAdmissao(item)?.nome ?? 'Pronto para admissao';
  }

  proximaEtapaAdmissao(item: any) {
    return this.etapasAdmissao(item).find((etapa: any) => etapa.status !== 'Concluida' && this.etapaLiberada(item, etapa));
  }

  avancarProximaEtapaAdmissao(item: any) {
    const etapa = this.proximaEtapaAdmissao(item);
    if (!etapa) {
      this.notificar('Nao ha etapa liberada para avancar.', 'info');
      return;
    }

    this.concluirEtapaAdmissao(item, etapa);
  }

  processoAdmitido(item: any) {
    return item?.status === 'Admitido';
  }

  processoFinalizado(item: any) {
    return item?.status === 'Admitido' || item?.status === 'Cancelado';
  }

  processoCancelado(item: any) {
    return item?.status === 'Cancelado';
  }

  selecionarEtapaAdmissao(item: any, etapa: any) {
    if (this.processoFinalizado(item)) return;
    this.admissaoEtapaSelecionada.set({ processoId: item.id, etapaNome: etapa.nome });
  }

  limparEtapaAdmissaoSelecionada() {
    this.admissaoEtapaSelecionada.set(null);
  }

  etapaSelecionada(item: any, etapa: any) {
    const selecionada = this.admissaoEtapaSelecionada();
    return selecionada?.processoId === item?.id && selecionada?.etapaNome === etapa?.nome;
  }

  etapaSelecionadaDoProcesso(item: any) {
    const selecionada = this.admissaoEtapaSelecionada();
    if (!selecionada || selecionada.processoId !== item?.id) return null;
    const etapaNome = selecionada.etapaNome;
    return this.etapasAdmissao(item).find((etapa: any) => etapa.nome === etapaNome) ?? null;
  }

  podeAvancarEtapa(item: any) {
    const etapa = this.etapaSelecionadaDoProcesso(item);
    if (!etapa || etapa.status === 'Concluida' || !this.etapaLiberada(item, etapa)) return false;
    if (this.etapaAnexarDocumentos(etapa)) return this.documentosAnexadosAdmissao(item).length > 0;
    return true;
  }

  podeVoltarEtapa(item: any) {
    const etapa = this.etapaSelecionadaDoProcesso(item);
    if (!etapa || this.processoFinalizado(item)) return false;

    const etapas = this.etapasAdmissao(item);
    const index = etapas.findIndex((x: any) => x.nome === etapa.nome);
    return index > 0 && (etapa.status === 'Concluida' || etapas[index - 1]?.status === 'Concluida');
  }

  avancarEtapaSelecionada(item: any) {
    const etapa = this.etapaSelecionadaDoProcesso(item);
    if (!etapa) return;
    if (this.etapaAnexarDocumentos(etapa) && !this.documentosAnexadosAdmissao(item).length) {
      this.notificar('Anexe um documento antes de avancar esta etapa.', 'error');
      return;
    }
    this.concluirEtapaAdmissao(item, etapa);
  }

  etapaAnexarDocumentos(etapa: any) {
    return etapa?.nome === 'Anexar documentos';
  }

  documentosAnexadosAdmissao(item: any) {
    return this.admissaoDocumentosAnexados()[item?.id] ?? [];
  }

  documentoAnexadoEhImagem(doc: any) {
    return String(doc?.tipoArquivo ?? doc?.tipo ?? '').startsWith('image/');
  }

  extensaoDocumentoAdmissao(doc: any) {
    const nome = doc?.nomeArquivo ?? doc?.nome ?? '';
    const extensao = nome.includes('.') ? nome.split('.').pop() : 'DOC';
    return String(extensao || 'DOC').slice(0, 4).toUpperCase();
  }

  selecionarDocumentoAdmissao(event: Event, item: any) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !item?.id) return;

    const form = new FormData();
    form.append('arquivo', file);

    this.http.post<any>(`${this.api}/admissoes/${item.id}/documentos-anexados`, form).subscribe({
      next: (documento) => {
        this.admissaoDocumentosAnexados.update((documentos) => ({
          ...documentos,
          [item.id]: [documento, ...(documentos[item.id] ?? [])],
        }));
        item.documentosAnexados = [documento, ...(item.documentosAnexados ?? [])];
        this.notificar('Documento anexado ao processo.', 'success');
      },
      error: () => this.notificar('Erro ao anexar documento ao processo.', 'error'),
    });
    input.value = '';
  }

  removerDocumentoAdmissao(item: any, doc: any) {
    if (!item?.id || !doc?.id) return;
    this.http.delete(`${this.api}/admissoes/${item.id}/documentos-anexados/${doc.id}`).subscribe({
      next: () => {
        this.admissaoDocumentosAnexados.update((documentos) => ({
          ...documentos,
          [item.id]: (documentos[item.id] ?? []).filter((documento) => documento.id !== doc.id),
        }));
        item.documentosAnexados = (item.documentosAnexados ?? []).filter((documento: any) => documento.id !== doc.id);
        this.notificar('Documento removido do processo.', 'info');
      },
      error: () => this.notificar('Erro ao remover documento do processo.', 'error'),
    });
  }

  setAdmissaoSearch(value: string) {
    this.admissaoSearch.set(value);
    this.admissaoPage.set(1);
  }

  setAdmissaoPage(page: number) {
    const bounded = Math.min(Math.max(page, 1), this.admissaoTotalPages());
    this.admissaoPage.set(bounded);
  }

  admissaoPageNumbers() {
    return Array.from({ length: this.admissaoTotalPages() }, (_value, index) => index + 1);
  }

  setDemissaoSearch(value: string) {
    this.demissaoSearch.set(value);
    this.demissaoPage.set(1);
  }

  setDemissaoPage(page: number) {
    const bounded = Math.min(Math.max(page, 1), this.demissaoTotalPages());
    this.demissaoPage.set(bounded);
  }

  demissaoPageNumbers() {
    return Array.from({ length: this.demissaoTotalPages() }, (_value, index) => index + 1);
  }

  voltarEtapaSelecionada(item: any) {
    const etapa = this.etapaSelecionadaDoProcesso(item);
    if (!etapa || !this.podeVoltarEtapa(item)) return;

    const etapas = this.etapasAdmissao(item);
    const index = etapas.findIndex((x: any) => x.nome === etapa.nome);
    const etapaParaReabrir = etapa.status === 'Concluida' ? etapa : etapas[index - 1];
    if (!etapaParaReabrir?.id) return;

    this.atualizarEtapaAdmissao(item, etapaParaReabrir, 'Pendente', 'Etapa anterior reaberta.');
  }

  concluirEtapaAdmissao(item: any, etapa: any) {
    if (!etapa.id || !this.etapaLiberada(item, etapa)) return;
    this.atualizarEtapaAdmissao(item, etapa, 'Concluida', 'Etapa concluida.');
  }

  cancelarProcessoAdmissao(item: any) {
    if (!item?.id || this.processoFinalizado(item)) return;

    this.http.post(`${this.api}/admissoes/${item.id}/cancelar`, {}).subscribe({
      next: () => {
        item.status = 'Cancelado';
        this.admissaoEtapaSelecionada.set(null);
        this.carregarDados();
        this.notificar('Processo de admissao cancelado.', 'success');
      },
      error: () => this.notificar('Erro ao cancelar processo de admissao.', 'error'),
    });
  }

  etapasDemissao(item: any) {
    const configs = this.etapasProcessosConfig()
      .filter((x) => x.tipoProcesso === 'Demissao' && x.ativa)
      .sort((a, b) => Number(a.ordem ?? 0) - Number(b.ordem ?? 0));
    const nomes = configs.length ? configs.map((x) => x.nome) : ['Aprovada pela direcao', 'Entrevista demissional', 'Exame demissional', 'Demissao efetivada'];
    const etapas = Array.isArray(item?.etapas) ? item.etapas : [];

    return nomes.map((nome) => etapas.find((x: any) => x.nome === nome) ?? { id: 0, nome, status: 'Pendente' });
  }

  etapaDemissaoLiberada(item: any, etapa: any) {
    if (this.processoDemissaoFinalizado(item)) return false;
    const etapas = this.etapasDemissao(item);
    const index = etapas.findIndex((x: any) => x.nome === etapa.nome);
    if (index <= 0) return true;
    return etapas.slice(0, index).every((x: any) => x.status === 'Concluida');
  }

  statusDemissao(item: any) {
    if (item?.status === 'Concluido') return 'Concluido';
    if (item?.status === 'Cancelado') return 'Cancelado';
    return this.etapasDemissao(item).find((etapa: any) => etapa.status !== 'Concluida')?.nome ?? 'Pronto para efetivar';
  }

  processoDemissaoFinalizado(item: any) {
    return item?.status === 'Concluido' || item?.status === 'Cancelado';
  }

  podeConcluirDemissao(item: any) {
    return !this.processoDemissaoFinalizado(item) && this.etapasDemissao(item).every((x: any) => x.status === 'Concluida');
  }

  selecionarEtapaDemissao(item: any, etapa: any) {
    if (this.processoDemissaoFinalizado(item)) return;
    this.demissaoEtapaSelecionada.set({ processoId: item.id, etapaNome: etapa.nome });
  }

  limparEtapaDemissaoSelecionada() {
    this.demissaoEtapaSelecionada.set(null);
  }

  etapaDemissaoSelecionada(item: any, etapa: any) {
    const selecionada = this.demissaoEtapaSelecionada();
    return selecionada?.processoId === item?.id && selecionada?.etapaNome === etapa?.nome;
  }

  etapaDemissaoSelecionadaDoProcesso(item: any) {
    const selecionada = this.demissaoEtapaSelecionada();
    if (!selecionada || selecionada.processoId !== item?.id) return null;
    return this.etapasDemissao(item).find((etapa: any) => etapa.nome === selecionada.etapaNome) ?? null;
  }

  podeAvancarEtapaDemissao(item: any) {
    const etapa = this.etapaDemissaoSelecionadaDoProcesso(item);
    return !!etapa && etapa.status !== 'Concluida' && this.etapaDemissaoLiberada(item, etapa);
  }

  podeVoltarEtapaDemissao(item: any) {
    const etapa = this.etapaDemissaoSelecionadaDoProcesso(item);
    if (!etapa || this.processoDemissaoFinalizado(item)) return false;

    const etapas = this.etapasDemissao(item);
    const index = etapas.findIndex((x: any) => x.nome === etapa.nome);
    return index > 0 && (etapa.status === 'Concluida' || etapas[index - 1]?.status === 'Concluida');
  }

  avancarEtapaDemissaoSelecionada(item: any) {
    const etapa = this.etapaDemissaoSelecionadaDoProcesso(item);
    if (!etapa || !this.podeAvancarEtapaDemissao(item)) return;
    this.atualizarEtapaDemissao(item, etapa, 'Concluida', 'Etapa demissional concluida.');
  }

  voltarEtapaDemissaoSelecionada(item: any) {
    const etapa = this.etapaDemissaoSelecionadaDoProcesso(item);
    if (!etapa || !this.podeVoltarEtapaDemissao(item)) return;

    const etapas = this.etapasDemissao(item);
    const index = etapas.findIndex((x: any) => x.nome === etapa.nome);
    const etapaParaReabrir = etapa.status === 'Concluida' ? etapa : etapas[index - 1];
    if (!etapaParaReabrir?.id) return;
    this.atualizarEtapaDemissao(item, etapaParaReabrir, 'Pendente', 'Etapa demissional reaberta.');
  }

  private atualizarEtapaAdmissao(item: any, etapa: any, status: string, mensagem: string) {
    const dataConclusao = status === 'Concluida' ? new Date().toISOString() : null;
    this.http
      .put(`${this.api}/admissoes/${item.id}/etapas/${etapa.id}`, { ...etapa, status, dataConclusao })
      .subscribe({
        next: () => {
          etapa.status = status;
          etapa.dataConclusao = dataConclusao;
          item.status = this.statusAdmissao(item);
          this.carregarDados();
          this.notificar(mensagem, 'success');
        },
        error: () => this.notificar('Erro ao atualizar etapa.', 'error'),
      });
  }

  private atualizarEtapaDemissao(item: any, etapa: any, status: string, mensagem: string) {
    const dataConclusao = status === 'Concluida' ? new Date().toISOString() : null;
    this.http
      .put(`${this.api}/demissoes/${item.id}/etapas/${etapa.id}`, { ...etapa, status, dataConclusao })
      .subscribe({
        next: () => {
          etapa.status = status;
          etapa.dataConclusao = dataConclusao;
          item.status = this.statusDemissao(item);
          this.carregarDados();
          this.notificar(mensagem, 'success');
        },
        error: () => this.notificar('Erro ao atualizar etapa demissional.', 'error'),
      });
  }

  iniciarFluxoCandidato(candidato: any) {
    this.abrirNovoProcessoAdmissao();
    this.selecionarVagaAdmissao(candidato.vagaId);
    this.selecionarCandidatoAdmissao(candidato.id);
  }

  nomeBeneficio(id?: number) {
    return this.beneficios().find((x) => x.id === id)?.nome ?? id ?? '-';
  }

  nomeTreinamento(id?: number) {
    return this.treinamentos().find((x) => x.id === id)?.titulo ?? id ?? '-';
  }

  imprimirFichaEpi(colaborador: any) {
    const retiradas = this.episDoColaborador(colaborador.id);
    const linhas = retiradas
      .map(
        (item) => `
          <tr>
            <td>${this.escapeHtml(this.nomeEpi(item.epiId))}</td>
            <td>${item.quantidade ?? 1}</td>
            <td>${this.formatDatePrint(item.dataRetirada)}</td>
            <td>${this.formatDatePrint(item.dataPrevistaTroca)}</td>
            <td>${this.formatDatePrint(item.dataDevolucao)}</td>
            <td>${this.escapeHtml(item.status ?? '')}</td>
            <td>${this.escapeHtml(item.observacoes ?? '')}</td>
          </tr>
        `,
      )
      .join('');

    this.imprimirHtml(`
      <h1>Ficha de entrega de EPIs</h1>
      <p><strong>Colaborador:</strong> ${this.escapeHtml(colaborador.nome)} &nbsp; <strong>CPF:</strong> ${this.escapeHtml(colaborador.cpf ?? '')}</p>
      <p><strong>Cargo:</strong> ${this.escapeHtml(this.nomeCargo(colaborador.cargoId))} &nbsp; <strong>Data:</strong> ${this.formatDatePrint(new Date())}</p>
      <table>
        <thead><tr><th>EPI</th><th>Qtd.</th><th>Retirada</th><th>Troca prevista</th><th>Devolução</th><th>Status</th><th>Observações</th></tr></thead>
        <tbody>${linhas || '<tr><td colspan="7">Nenhum EPI retirado.</td></tr>'}</tbody>
      </table>
      <div class="signatures">
        <span>Assinatura do colaborador</span>
        <span>Responsável pela entrega</span>
      </div>
    `);
  }

  imprimirDocumentosAdmissao(colaborador: any) {
    const data = this.formatDatePrint(new Date());
    const docs = [
      'Termo de Ciência, Concordância e Adesão à Política Interna de Pausas para Café',
      'Termo de Responsabilidade de Chaves e Senhas de Alarmes kONECT',
      'Termo ciência Refeição KONECT',
      'Alelo Alimentação - Termo Recebimento + Absenteísmo KONECT',
      'Termo Celulares',
      'Aditivo Contrato de Trabalho - KONECT - Imagem LGPD',
    ];

    this.imprimirHtml(
      docs
        .map(
          (titulo) => `
            <section class="document-page">
              <h1>${this.escapeHtml(titulo)}</h1>
              <p><strong>Colaborador:</strong> ${this.escapeHtml(colaborador.nome)}</p>
              <p><strong>CPF:</strong> ${this.escapeHtml(colaborador.cpf ?? '')}</p>
              <p><strong>Data:</strong> ${data}</p>
              <p class="placeholder">Documento institucional para conferência, ciência e assinatura ao concluir o processo de admissão.</p>
              <div class="signature">Assinatura do colaborador</div>
            </section>
          `,
        )
        .join(''),
    );
  }

  imprimirListaPresenca(treinamento: any) {
    const participantes = this.treinamentosColaboradores().filter((x) => x.treinamentoId === treinamento.id);
    const linhas = participantes
      .map(
        (item) => `
          <tr>
            <td>${this.escapeHtml(this.nomeColaborador(item.colaboradorId))}</td>
            <td>${this.escapeHtml(treinamento.instrutor ?? '')}</td>
            <td>${this.escapeHtml(treinamento.titulo ?? '')}</td>
            <td>${item.presente ? 'Presente' : 'Pendente'}</td>
            <td></td>
          </tr>
        `,
      )
      .join('');

    this.imprimirHtml(`
      <h1>Lista de presença</h1>
      <p><strong>Treinamento:</strong> ${this.escapeHtml(treinamento.titulo ?? '')}</p>
      <p><strong>Instrutor:</strong> ${this.escapeHtml(treinamento.instrutor ?? '')} &nbsp; <strong>Data:</strong> ${this.formatDatePrint(treinamento.dataInicio)}</p>
      <table>
        <thead><tr><th>Colaborador</th><th>Instrutor</th><th>Treinamento</th><th>Status</th><th>Assinatura</th></tr></thead>
        <tbody>${linhas || '<tr><td colspan="5">Nenhum participante vinculado.</td></tr>'}</tbody>
      </table>
    `);
  }

  setModule(id: string) {
    this.activeModule.set(id);
    if (this.cadastroModules().some((module) => module.id === id)) this.cadastroMenuOpen.set(true);
    if (this.financeiroModules().some((module) => module.id === id)) this.financeiroMenuOpen.set(true);
    if (this.peopleModules().some((module) => module.id === id)) this.peopleMenuOpen.set(true);
    if (this.recrutamentoModules().some((module) => module.id === id)) {
      this.recrutamentoMenuOpen.set(true);
      this.recrutamentoTab.set('pesquisa');
      if (id === 'recrutamento-vagas') this.recrutamentoCadastro.set('vagas');
      if (id === 'recrutamento-candidatos') this.recrutamentoCadastro.set('candidatos');
      if (id === 'recrutamento-curriculos') this.recrutamentoCadastro.set('curriculos');
    }
    this.search.set('');
    this.userMenuOpen.set(false);
  }

  private salvarCrud<T extends { id?: number }>(endpoint: string, payload: T, reset: () => void) {
    const body = this.normalizePayload(payload);
    const request = payload.id
      ? this.http.put(`${this.api}/${endpoint}/${payload.id}`, body)
      : this.http.post(`${this.api}/${endpoint}`, body);

    request.subscribe({
      next: () => {
        reset();
        this.carregarDados();
        this.notificar(payload.id ? 'Registro atualizado com sucesso.' : 'Registro criado com sucesso.', 'success');
      },
      error: () => this.notificar(`Erro ao salvar ${endpoint}.`, 'error'),
    });
  }

  private sincronizarDocumentosAnexadosAdmissao(processos: any[]) {
    const documentosPorProcesso = processos.reduce<Record<number, any[]>>((acc, processo) => {
      if (processo?.id) acc[processo.id] = processo.documentosAnexados ?? [];
      return acc;
    }, {});
    this.admissaoDocumentosAnexados.set(documentosPorProcesso);
  }

  private concluirAdmissaoComColaborador(colaborador: any, admissao: any) {
    if (!admissao?.id || !colaborador?.id) {
      this.registrarDocumentosAdmissaoNoColaborador(colaborador, admissao);
      return;
    }

    this.http.post(`${this.api}/admissoes/${admissao.id}/admitir`, { colaboradorId: colaborador.id }).subscribe({
      next: () => this.registrarDocumentosAdmissaoNoColaborador(colaborador, admissao),
      error: () => {
        this.admissaoSelecionada = null;
        this.carregarDados();
        this.notificar('Colaborador cadastrado, mas não foi possível concluir a admissão.', 'error');
      },
    });
  }

  private registrarDocumentosAdmissaoNoColaborador(colaborador: any, admissao: any) {
    const documentos = this.documentosAnexadosAdmissao(admissao);
    if (!colaborador?.id || !documentos.length) {
      this.admissaoSelecionada = null;
      this.carregarDados();
      this.notificar('Colaborador cadastrado pela admissão.', 'success');
      return;
    }

    const requests = documentos.map((doc) =>
      this.http.post(`${this.api}/documentosColaboradores`, {
        colaboradorId: colaborador.id,
        tipoDocumento: 'Documento admissional',
        nomeArquivo: doc.nomeArquivo,
        urlArquivo: doc.urlArquivo,
        obrigatorio: true,
        validado: false,
      }),
    );

    forkJoin(requests).subscribe({
      next: () => {
        this.admissaoSelecionada = null;
        this.carregarDados();
        this.notificar('Colaborador cadastrado e documentos vinculados.', 'success');
      },
      error: () => {
        this.admissaoSelecionada = null;
        this.carregarDados();
        this.notificar('Colaborador cadastrado, mas não foi possível vincular todos os documentos.', 'error');
      },
    });
  }

  private aplicarLogin(response: AuthResponse) {
    sessionStorage.setItem('uniflow.token', response.token);
    sessionStorage.setItem('uniflow.usuario', JSON.stringify(response.usuario));
    this.token.set(response.token);
    this.usuario.set(response.usuario);
    this.cadastroMenuOpen.set(false);
    this.peopleMenuOpen.set(false);
    this.financeiroMenuOpen.set(false);
    this.prepararPerfil();
    this.reiniciarTimerInatividade();
    this.carregarDados();
  }

  private limparSessaoPersistenteAntiga() {
    localStorage.removeItem('uniflow.token');
    localStorage.removeItem('uniflow.usuario');
    localStorage.removeItem('uniflow.empresa');
  }

  private registrarMonitorInatividade() {
    const events = ['click', 'keydown', 'mousemove', 'scroll', 'touchstart'];
    for (const event of events) {
      window.addEventListener(event, () => this.reiniciarTimerInatividade(), { passive: true });
    }
  }

  private reiniciarTimerInatividade() {
    if (!this.isLogged()) return;
    this.pararTimerInatividade();
    this.inactivityTimeoutId = setTimeout(() => {
      this.logout();
      this.notificar('Sessao encerrada por inatividade.', 'info');
    }, this.sessionTimeoutMs);
  }

  private pararTimerInatividade() {
    if (!this.inactivityTimeoutId) return;
    clearTimeout(this.inactivityTimeoutId);
    this.inactivityTimeoutId = undefined;
  }

  private verificarAdminInicial() {
    this.http.get<{ exists: boolean }>(`${this.api}/auth/system-admin-exists`).subscribe({
      next: (response) => this.systemAdminExists.set(!!response.exists),
      error: () => this.systemAdminExists.set(true),
    });
  }

  private prepararPerfil() {
    const usuario = this.usuario();
    this.profileForm = { nome: usuario?.nome ?? '', email: usuario?.email ?? '' };
  }

  private notificar(texto: string, tipo: MessageType = 'info') {
    this.mensagem.set(texto);
    this.mensagemTipo.set(tipo);
    setTimeout(() => {
      if (this.mensagem() === texto) this.mensagem.set('');
    }, 4200);
  }

  private setLoading(value: boolean) {
    this.loading.set(value);
    if (value) {
      void this.spinner.show('uniflow-loading');
      return;
    }
    void this.spinner.hide('uniflow-loading');
  }

  private filterByTerm<T extends Record<string, any>>(items: T[], fields: string[]) {
    const term = this.search().trim().toLowerCase();
    if (!term) return items;
    return items.filter((item) =>
      fields.some((field) => String(item[field] ?? '').toLowerCase().includes(term)),
    );
  }

  private filterBySignal<T extends Record<string, any>>(
    items: T[],
    value: string,
    fields: string[],
    extra?: (item: T) => string,
  ) {
    const term = value.trim().toLowerCase();
    if (!term) return items;
    return items.filter((item) => {
      const directMatch = fields.some((field) => String(item[field] ?? '').toLowerCase().includes(term));
      return directMatch || String(extra?.(item) ?? '').toLowerCase().includes(term);
    });
  }

  private scrollTop() {
    setTimeout(() => document.querySelector('.workspace')?.scrollTo({ top: 0, behavior: 'smooth' }), 0);
  }

  private toDateInput(value: any) {
    return value ? new Date(value).toISOString().slice(0, 10) : '';
  }

  private toDateTimeInput(value: any) {
    return value ? new Date(value).toISOString().slice(0, 16) : '';
  }

  formatDatePrint(value: any) {
    if (!value) return '';
    return new Date(value).toLocaleDateString('pt-BR');
  }

  private escapeHtml(value: any) {
    return String(value ?? '')
      .replaceAll('&', '&amp;')
      .replaceAll('<', '&lt;')
      .replaceAll('>', '&gt;')
      .replaceAll('"', '&quot;')
      .replaceAll("'", '&#039;');
  }

  private imprimirHtml(content: string) {
    const janela = window.open('', '_blank', 'width=980,height=720');
    if (!janela) {
      this.notificar('Não foi possível abrir a janela de impressão.', 'error');
      return;
    }

    janela.document.write(`
      <!doctype html>
      <html lang="pt-BR">
        <head>
          <meta charset="utf-8" />
          <title>Impressão</title>
          <style>
            body { color: #111; font-family: Arial, sans-serif; margin: 28px; }
            h1 { font-size: 22px; margin: 0 0 16px; text-transform: uppercase; }
            p { margin: 8px 0; }
            table { width: 100%; border-collapse: collapse; margin-top: 18px; }
            th, td { border: 1px solid #333; padding: 8px; font-size: 12px; text-align: left; vertical-align: top; }
            th { background: #f2f2f2; }
            .signatures { display: grid; grid-template-columns: 1fr 1fr; gap: 48px; margin-top: 64px; }
            .signatures span, .signature { border-top: 1px solid #111; padding-top: 8px; text-align: center; }
            .signature { margin-top: 80px; }
            .document-page { min-height: 92vh; page-break-after: always; }
            .document-template-page { max-width: 760px; margin: 0 auto; }
            .document-template-body { font-size: 13px; line-height: 1.55; white-space: normal; }
            .document-template-body p { margin: 0 0 12px; }
            .document-template-body p:first-child { font-weight: 700; text-align: center; text-transform: uppercase; margin-bottom: 22px; }
            .placeholder { margin-top: 32px; line-height: 1.6; }
            @media print { button { display: none; } body { margin: 18mm; } }
          </style>
        </head>
        <body>${content}<script>window.onload = () => window.print();</script></body>
      </html>
    `);
    janela.document.close();
  }

  private normalizePayload<T>(payload: T): T {
    return JSON.parse(
      JSON.stringify(payload, (_key, value) => {
        if (value === '') return null;
        return value;
      }),
    ) as T;
  }

  limparErroCpfColaborador() {
    if (this.cpfColaboradorDuplicado()) this.cpfColaboradorDuplicado.set(false);
  }

  private cpfColaboradorJaExiste() {
    const cpf = this.normalizarCpf(this.colaboradorForm.cpf);
    if (!cpf) return false;

    return this.colaboradores().some((colaborador) => {
      if (colaborador.id === this.colaboradorForm.id) return false;
      return this.normalizarCpf(colaborador.cpf) === cpf;
    });
  }

  private marcarCpfDuplicado() {
    this.cpfColaboradorDuplicado.set(true);
    this.notificar('Já existe um colaborador cadastrado com este CPF.', 'error');
    setTimeout(() => {
      const input = document.querySelector<HTMLInputElement>('.cpf-duplicado, input[name="colabCpf"], input[name="admColabCpf"]');
      input?.focus();
    }, 0);
  }

  private tratarErroSalvarColaborador(error: any, fallback = 'Erro ao salvar colaborador.') {
    const texto = JSON.stringify(error?.error ?? error ?? '').toLowerCase();
    if (error?.status === 409 || texto.includes('cpf') || texto.includes('duplicate') || texto.includes('unique')) {
      this.marcarCpfDuplicado();
      return;
    }

    this.notificar(fallback, 'error');
  }

  private normalizarCpf(value: any) {
    return String(value ?? '').replace(/\D/g, '');
  }

  private normalizeDateTime(payload: any, fields: string[]) {
    for (const field of fields) {
      if (payload[field]) payload[field] = new Date(payload[field]).toISOString();
    }
  }

  private ultimosMeses(quantidade: number) {
    const formatter = new Intl.DateTimeFormat('pt-BR', { month: 'short' });
    const hoje = new Date();

    return Array.from({ length: quantidade }, (_value, index) => {
      const data = new Date(hoje.getFullYear(), hoje.getMonth() - (quantidade - 1 - index), 1);
      const label = formatter.format(data).replace('.', '');

      return {
        key: `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, '0')}`,
        label: label.charAt(0).toUpperCase() + label.slice(1),
        ano: data.getFullYear(),
        mes: data.getMonth(),
      };
    });
  }

  private mesDaData(value: any) {
    if (!value) return '';
    const data = new Date(value);
    if (Number.isNaN(data.getTime())) return '';
    return `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, '0')}`;
  }

  private prepararStatusColaborador() {
    this.colaboradorForm.ativo = this.colaboradorForm.status === 'Ativo';
    if (this.colaboradorForm.status === 'Demitido' && !this.colaboradorForm.dataDemissao) {
      this.colaboradorForm.dataDemissao = new Date().toISOString().slice(0, 10);
    }
    if (this.colaboradorForm.status === 'Ativo') {
      this.colaboradorForm.dataDemissao = '';
    }
  }

  private criarAdmissaoParaCandidato(candidato: any) {
    const vaga = this.vagas().find((x) => x.id === candidato.vagaId);
    const admissao = {
      colaboradorId: undefined,
      nomeCandidato: candidato.nome,
      email: candidato.email,
      telefone: candidato.telefone,
      cargoPretendido: vaga?.titulo ?? '',
      dataPrevistaAdmissao: new Date().toISOString(),
      status: 'Em andamento',
    };

    this.http.post(`${this.api}/admissoes`, this.normalizePayload(admissao)).subscribe({
      next: () => {
        this.carregarDados();
        this.notificar('Candidato cadastrado e fluxo de admissao iniciado.', 'success');
      },
      error: () => {
        this.carregarDados();
        this.notificar('Candidato salvo, mas nao foi possivel iniciar a admissao.', 'error');
      },
    });
  }

  private vagaDaAdmissao(item?: any) {
    if (!item) return undefined;
    return this.vagas().find((x) => x.titulo === item.cargoPretendido);
  }

  novaEmpresa(): Empresa {
    return {
      razaoSocial: '',
      nomeFantasia: '',
      cnpj: '',
      telefone: '',
      email: '',
      endereco: '',
      cidade: '',
      estado: '',
      cep: '',
      ativo: true,
    };
  }

  novoContrato(): Contrato {
    return {
      empresaId: 0,
      planoId: null,
      plano: 'Starter',
      status: 'Ativo',
      dataInicio: new Date().toISOString().slice(0, 10),
      dataFim: '',
      limiteColaboradores: 50,
      valorMensal: 0,
      observacoes: '',
    };
  }

  novoPlano(): Plano {
    return {
      nome: '',
      prazoDias: 30,
      limiteColaboradores: 50,
      valorCobranca: 0,
      status: 'Ativo',
      observacoes: '',
    };
  }

  novaCobranca(): Cobranca {
    const hoje = new Date();
    const vencimento = new Date();
    vencimento.setDate(vencimento.getDate() + 30);
    return {
      empresaId: 0,
      contratoId: 0,
      descricao: '',
      valor: 0,
      dataGeracao: hoje.toISOString().slice(0, 10),
      dataVencimento: vencimento.toISOString().slice(0, 10),
      status: 'Pendente',
    };
  }

  novoUsuario(): UsuarioCreate {
    return {
      empresaId: undefined,
      colaboradorId: undefined,
      nome: '',
      login: '',
      email: '',
      senha: '',
      role: 'EmpresaAdmin',
      ativo: true,
    };
  }

  novaFilial() {
    return { nome: '', cnpj: '', endereco: '', cidade: '', estado: '', telefone: '', ativa: true };
  }

  novoDepartamento() {
    return { nome: '', descricao: '', gestorId: undefined, ativo: true };
  }

  novoCargo() {
    return { nome: '', descricao: '', nivel: '', salarioBase: 0, ativo: true };
  }

  private novoColaborador() {
    return {
      filialId: undefined,
      departamentoId: undefined,
      cargoId: undefined,
      nome: '',
      cpf: '',
      rg: '',
      telefone: '',
      email: '',
      endereco: '',
      bairro: '',
      cidade: '',
      estado: '',
      cep: '',
      dataNascimento: '',
      sexo: '',
      estadoCivil: '',
      dataAdmissao: new Date().toISOString().slice(0, 10),
      dataDemissao: '',
      tipoContrato: 'CLT',
      salario: 0,
      cargaHorariaSemanal: 44,
      matricula: '',
      pis: '',
      ctps: '',
      status: 'Ativo',
      fotoUrl: '',
      observacoes: '',
      ativo: true,
    };
  }

  novoBeneficio() {
    return { nome: '', descricao: '', valorPadrao: 0, ativo: true };
  }

  novaVaga() {
    return {
      departamentoId: undefined,
      cargoId: undefined,
      titulo: '',
      descricao: '',
      quantidade: undefined,
      salario: undefined,
      status: 'Aberta',
      dataEncerramento: '',
    };
  }

  novoCandidato() {
    return {
      vagaId: undefined,
      nome: '',
      cpf: '',
      telefone: '',
      email: '',
      curriculoUrl: '',
      linkedin: '',
      status: 'Recebido',
      observacoes: '',
    };
  }

  novoCurriculo() {
    return {
      nome: '',
      telefone: '',
      email: '',
    };
  }

  private novoTreinamento() {
    return {
      titulo: '',
      descricao: '',
      instrutor: '',
      dataInicio: new Date().toISOString().slice(0, 16),
      dataFim: '',
      cargaHoraria: 1,
      metodoAvaliacaoEficacia: '',
      eficaz: false,
      status: 'Planejado',
      obrigatorio: false,
    };
  }

  private novoTreinamentoColaborador() {
    return {
      treinamentoId: undefined,
      colaboradorId: undefined,
      presente: false,
      status: 'Inscrito',
      observacoes: '',
    };
  }

  novoEpi() {
    return {
      nome: '',
      ca: '',
      descricao: '',
      periodicidadeTrocaDias: 180,
      ativo: true,
    };
  }

  novoCargoEpi() {
    return {
      cargoId: undefined,
      epiId: undefined,
      quantidadePadrao: 1,
      obrigatorio: true,
    };
  }

  novoColaboradorEpi() {
    return {
      colaboradorId: undefined,
      epiId: undefined,
      quantidade: 1,
      dataRetirada: new Date().toISOString().slice(0, 10),
      dataPrevistaTroca: '',
      dataDevolucao: '',
      status: 'Retirado',
      observacoes: '',
    };
  }

  novaFerramentaAcesso() {
    return {
      nome: '',
      tipo: 'Tag de acesso',
      identificador: '',
      descricao: '',
      ativa: true,
    };
  }

  novoColaboradorFerramentaAcesso() {
    return {
      colaboradorId: undefined,
      ferramentaAcessoId: undefined,
      dataEntrega: new Date().toISOString().slice(0, 10),
      dataDevolucao: '',
      status: 'Entregue',
      observacoes: '',
    };
  }

  novaEtapaProcesso() {
    return {
      tipoProcesso: 'Admissao',
      nome: '',
      descricao: '',
      ordem: 1,
      primeiraEtapaConcluida: false,
      ativa: true,
    };
  }

  private novaAdmissao() {
    return {
      vagaId: undefined,
      candidatoId: undefined,
      colaboradorId: undefined,
      nomeCandidato: '',
      email: '',
      telefone: '',
      cargoPretendido: '',
      dataPrevistaAdmissao: new Date().toISOString().slice(0, 10),
      status: 'Em andamento',
    };
  }

  private novaDemissao() {
    return {
      colaboradorId: undefined,
      tipoDemissao: 'Sem justa causa',
      dataSolicitacao: new Date().toISOString().slice(0, 10),
      dataDesligamento: '',
      status: 'Em andamento',
      motivo: '',
      observacoes: '',
    };
  }

  private novaFerias() {
    const hoje = new Date().toISOString().slice(0, 10);
    return {
      colaboradorId: undefined,
      periodoAquisitivoInicio: hoje,
      periodoAquisitivoFim: hoje,
      dataInicio: hoje,
      dataFim: hoje,
      dias: 30,
      abono: false,
      status: 'Pendente',
    };
  }

  private novoPonto() {
    return {
      colaboradorId: undefined,
      dataHora: new Date().toISOString().slice(0, 16),
      tipo: 'Entrada',
      latitude: undefined,
      longitude: undefined,
      ip: '',
      observacao: '',
    };
  }

  private novoDocumento() {
    return {
      colaboradorId: undefined,
      tipoDocumento: '',
      nomeArquivo: '',
      urlArquivo: '',
      obrigatorio: false,
      validado: false,
    };
  }

  private novaSolicitacao() {
    return {
      colaboradorId: this.usuario()?.colaboradorId ?? undefined,
      tipo: '',
      titulo: '',
      descricao: '',
      prioridade: 'Normal',
      status: 'Enviada',
      respostaRh: '',
    };
  }

  novoBeneficioColaborador() {
    return {
      colaboradorId: undefined,
      beneficioId: undefined,
      valor: 0,
      dataInicio: new Date().toISOString().slice(0, 10),
      dataFim: '',
      ativo: true,
    };
  }

  private resolveApiBase() {
    const origin = window.location.origin;
    const isAngularDevServer = ['localhost:4200', '127.0.0.1:4200'].includes(window.location.host);
    return isAngularDevServer ? 'http://localhost:5036/api' : `${origin}/api`;
  }
}
