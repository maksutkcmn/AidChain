// src/config.ts

// ====== AidChain V8 - VERİFİER ÇIKARMA KALDIRILDI ======
// V8 Yenilikler:
// - Bir kez eklenen verifier (DAO üyesi) blockchain'den ÇIKARILAMAZ
// - remove_verifier fonksiyonu kaldırıldı
// - Güvenlik: Admin bile keyfi olarak DAO üyelerini çıkaramaz
//
// V7'den korunan özellikler:
// - Adminler/verifier'lar yardım başvurusu YAPAMAZ
// - İkametgah + Gelir belgesi gerekli
// - DAO oylama sistemi

export const AIDCHAIN_PACKAGE_ID =
  '0xc974b1f31dc9afdbe11f7b08ca15155f4d1a9aed8524138f42ee82833065f07d';

export const AIDCHAIN_REGISTRY_ID =
  '0x7a170f06ef97d0254b5a08f694f54db71dcaf767f090ef1edade366e5e311889';

export const REGISTRY_INITIAL_SHARED_VERSION = 670251442;

// Walrus Testnet Aggregator/Publisher URLs
export const WALRUS_AGGREGATOR_URL = 'https://aggregator.walrus-testnet.walrus.space';
export const WALRUS_PUBLISHER_URL = 'https://publisher.walrus-testnet.walrus.space';

// DAO Config Defaults (blockchain'den de okunur)
export const DEFAULT_VOTING_PERIOD = 10;     // 10 epoch
export const DEFAULT_QUORUM_PERCENT = 50;    // %50 katılım
export const DEFAULT_APPROVAL_PERCENT = 60;  // %60 onay

